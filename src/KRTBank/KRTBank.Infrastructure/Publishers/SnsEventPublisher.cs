using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using KRTBank.Application.Interfaces;

namespace KRTBank.Infrastructure.Publishers;

public class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _sns;
    
    public SnsEventPublisher(IAmazonSimpleNotificationService sns)
    {
        _sns = sns;
    }

    public async Task PublishAsync<T>(string topicArn, T message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = json
        };

        await _sns.PublishAsync(request, cancellationToken);
    }
}