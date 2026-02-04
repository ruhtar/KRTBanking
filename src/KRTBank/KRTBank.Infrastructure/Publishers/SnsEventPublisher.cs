using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using KRTBank.Application.Interfaces;

namespace KRTBank.Infrastructure.Publishers;

public class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _sns;
    private const string Arn = "arn:aws:sns:us-east-1:775442788781:krtbank-account-events"; //TODO: USAR UM SECRETS
    
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    public SnsEventPublisher(IAmazonSimpleNotificationService sns)
    {
        _sns = sns;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, JsonOptions);

        var request = new PublishRequest
        {
            TopicArn = Arn,
            Message = json
        };

        await _sns.PublishAsync(request, cancellationToken);
    }
}