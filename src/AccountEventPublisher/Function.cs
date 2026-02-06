using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;
using MessageAttributeValue = Amazon.SimpleNotificationService.Model.MessageAttributeValue;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AccountEventPublisher;

public class Function
{
    private readonly AmazonSimpleNotificationServiceClient _sns;
    private readonly AmazonSQSClient _sqs;
    private readonly string _topicArn;
    private readonly string _dlqQueueUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public Function()
    {
        var snsConfig = new AmazonSimpleNotificationServiceConfig
        {
            RetryMode = RequestRetryMode.Standard, 
            MaxErrorRetry = 3
        };
        _sns = new AmazonSimpleNotificationServiceClient(snsConfig);

        _sqs = new AmazonSQSClient();

        _topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")
            ?? throw new Exception("SNS_TOPIC_ARN not configured");

        _dlqQueueUrl = Environment.GetEnvironmentVariable("DLQ_QUEUE_URL") 
            ?? throw new Exception("DLQ_QUEUE_URL not configured");
    }

    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing {dynamoEvent.Records.Count} stream records...");

        foreach (var record in dynamoEvent.Records)
        {
            string? message = null;          
            IAccountEvent? accountEvent = null;

            try
            {
                var eventName = record.EventName;

                context.Logger.LogInformation($"Record received: {JsonSerializer.Serialize(record, JsonOptions)}");

                accountEvent = eventName switch
                {
                    "INSERT" => BuildCreatedEvent(record),
                    "MODIFY" => BuildUpdatedEvent(record),
                    "REMOVE" => BuildDeletedEvent(record),
                    _ => null
                };

                if (accountEvent is null)
                    continue;

                message = JsonSerializer.Serialize(accountEvent, accountEvent.GetType(), JsonOptions);

                context.Logger.LogInformation($"Message to be published: {message}");

                //throw new Exception("Teste de DLQ");

                await _sns.PublishAsync(new PublishRequest
                {
                    TopicArn = _topicArn,
                    Message = message,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        ["EventType"] = new() { DataType = "String", StringValue = accountEvent.Type },
                        ["ContentType"] = new() { DataType = "String", StringValue = "application/json" }
                    }
                });

                context.Logger.LogInformation($"Published event: {accountEvent.GetType().Name}");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Failed to publish. Exception={ex}");

                await SendToDlqAsync(record, message, ex, context);

                continue;
            }
        }
    }

    private static AccountCreatedEvent BuildCreatedEvent(DynamoDBEvent.DynamodbStreamRecord record)
    {
        var newImage = record.Dynamodb.NewImage;

        return new AccountCreatedEvent(
            Type: "AccountCreated",
            AccountId: newImage["Id"].S,
            HolderName: newImage["HolderName"].S,
            Cpf: newImage["Cpf"].S,
            Status: newImage["Status"].N,
            Timestamp: DateTime.UtcNow
        );
    }

    private static AccountUpdatedEvent BuildUpdatedEvent(DynamoDBEvent.DynamodbStreamRecord record)
    {
        var oldImage = record.Dynamodb.OldImage;
        var newImage = record.Dynamodb.NewImage;

        return new AccountUpdatedEvent(
            Type: "AccountUpdated",
            AccountId: newImage["Id"].S,
            OldName: oldImage["HolderName"].S,
            NewName: newImage["HolderName"].S,
            NewStatus: newImage["Status"].N,
            OldStatus: oldImage["Status"].N,
            Timestamp: DateTime.UtcNow
        );
    }

    private static AccountDeletedEvent BuildDeletedEvent(DynamoDBEvent.DynamodbStreamRecord record)
    {
        var oldImage = record.Dynamodb.OldImage;

        return new AccountDeletedEvent(
            Type: "AccountDeleted",
            AccountId: oldImage["Id"].S,
            Timestamp: DateTime.UtcNow
        );
    }

    public interface IAccountEvent
    {
        string Type { get; }
        DateTime Timestamp { get; }
    }

    public record AccountCreatedEvent(
        string Type,
        string AccountId,
        string HolderName,
        string Cpf,
        string Status,
        DateTime Timestamp
    ) : IAccountEvent;

    public record AccountUpdatedEvent(
        string Type,
        string AccountId,
        string OldName,
        string NewName,
        string OldStatus,
        string NewStatus,
        DateTime Timestamp
    ) : IAccountEvent;

    public record AccountDeletedEvent(
        string Type,
        string AccountId,
        DateTime Timestamp
    ) : IAccountEvent;


    private async Task SendToDlqAsync(
        DynamodbStreamRecord record,
        string? messageToPublish,
        Exception ex,
        ILambdaContext context)
    {
        try
        {
            JsonElement? messageJson = null;
            if (!string.IsNullOrWhiteSpace(messageToPublish))
            {
                using var doc = JsonDocument.Parse(messageToPublish);
                messageJson = doc.RootElement.Clone();
            }

            var dlqPayload = new
            {
                error = new
                {
                    message = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                },
                stream = new
                {
                    eventName = record.EventName,
                },
                publish = new
                {
                    target = "SNS",
                    topicArn = _topicArn,
                    message = (object?)messageJson ?? messageToPublish
                },
                meta = new
                {
                    timestampUtc = DateTime.UtcNow
                }
            };

            var dlqBody = JsonSerializer.Serialize(dlqPayload, JsonOptions);

            await _sqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _dlqQueueUrl,
                MessageBody = dlqBody
            });
        }
        catch (Exception dlqEx)
        {
            context.Logger.LogError($"Failed to publish on DLQ. Exception={dlqEx}");
        }
    }
}

