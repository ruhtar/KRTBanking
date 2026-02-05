using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AccountEventPublisher;

public class Function
{
    private readonly AmazonSimpleNotificationServiceClient _sns;
    private readonly string _topicArn;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Function()
    {
        var snsConfig = new AmazonSimpleNotificationServiceConfig
        {
            RetryMode = RequestRetryMode.Standard, 
            MaxErrorRetry = 3
        };
        _sns = new AmazonSimpleNotificationServiceClient(snsConfig);
        _topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")
            ?? throw new Exception("SNS_TOPIC_ARN not configured");
    }

    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing {dynamoEvent.Records.Count} stream records...");

        foreach (var record in dynamoEvent.Records)
        {
            try
            {
                var eventName = record.EventName;

                context.Logger.LogInformation($"Record received: {JsonSerializer.Serialize(record, JsonOptions)}");

                IAccountEvent? accountEvent = eventName switch
                {
                    "INSERT" => BuildCreatedEvent(record),
                    "MODIFY" => BuildUpdatedEvent(record),
                    "REMOVE" => BuildDeletedEvent(record),
                    _ => null
                };

                if (accountEvent is null)
                    continue;

                var message = JsonSerializer.Serialize(accountEvent, accountEvent.GetType(), JsonOptions);

                context.Logger.LogInformation($"Message to be published: {message}");

                await _sns.PublishAsync(new PublishRequest
                {
                    TopicArn = _topicArn,
                    Message = message,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        // Use o "Type" do evento (estável e legível)
                        ["EventType"] = new() { DataType = "String", StringValue = accountEvent.Type },

                        // Opcional (pode ser útil pro consumidor)
                        ["ContentType"] = new() { DataType = "String", StringValue = "application/json" }
                    }
                });

                context.Logger.LogInformation($"Published event: {accountEvent.GetType().Name}");
            }
            catch (Exception ex)
            {
                // Após o SDK esgotar as retentativas, cai aqui
                context.Logger.LogError($"Failed to publish after retries. Exception={ex}");
                throw; // importante: mantém falha para a Lambda/DLQ/redrive lidarem
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
            Status: newImage["Status"].N,
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

    // Contrato comum dos eventos
    public interface IAccountEvent
    {
        string Type { get; }
        DateTime Timestamp { get; }
    }

    // DTOs fortes (acabou o anonymous type)
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
        string Status,
        DateTime Timestamp
    ) : IAccountEvent;

    public record AccountDeletedEvent(
        string Type,
        string AccountId,
        DateTime Timestamp
    ) : IAccountEvent;
}

