using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AccountEventPublisher;

public class Function
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly string _topicArn;

    public Function()
    {
        _sns = new AmazonSimpleNotificationServiceClient();
        _topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")
            ?? throw new Exception("SNS_TOPIC_ARN not configured");
    }

    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing {dynamoEvent.Records.Count} stream records...");

        foreach (var record in dynamoEvent.Records)
        {
            var eventName = record.EventName;

            context.Logger.LogInformation($"EventID: {record.EventID} | Type: {eventName}");

            object? payload = eventName switch
            {
                "INSERT" => BuildCreatedEvent(record),
                "MODIFY" => BuildUpdatedEvent(record),
                "REMOVE" => BuildDeletedEvent(record),
                _ => null
            };

            if (payload is null)
                continue;

            var message = JsonSerializer.Serialize(payload);

            await _sns.PublishAsync(new PublishRequest
            {
                TopicArn = _topicArn,
                Message = message,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["EventType"] = new() { DataType = "String", StringValue = payload.GetType().Name }
                }
            });

            context.Logger.LogInformation($"Published event: {payload.GetType().Name}");
        }
    }

    private static object BuildCreatedEvent(DynamoDBEvent.DynamodbStreamRecord record)
    {
        var newImage = record.Dynamodb.NewImage;

        return new
        {
            Type = "AccountCreated",
            AccountId = newImage["Id"].S,
            HolderName = newImage["HolderName"].S,
            Cpf = newImage["Cpf"].S,
            Status = newImage["Status"].S,
            Timestamp = DateTime.UtcNow
        };
    }

    private static object BuildUpdatedEvent(DynamoDBEvent.DynamodbStreamRecord record)
    {
        var oldImage = record.Dynamodb.OldImage;
        var newImage = record.Dynamodb.NewImage;

        return new
        {
            Type = "AccountUpdated",
            AccountId = newImage["Id"].S,
            OldName = oldImage["HolderName"].S,
            NewName = newImage["HolderName"].S,
            IsActive = newImage["Status"].S == "Active",
            Timestamp = DateTime.UtcNow
        };
    }

    private static object BuildDeletedEvent(DynamoDBEvent.DynamodbStreamRecord record)
    {
        var oldImage = record.Dynamodb.OldImage;

        return new
        {
            Type = "AccountDeleted",
            AccountId = oldImage["Id"].S,
            Timestamp = DateTime.UtcNow
        };
    }
}
