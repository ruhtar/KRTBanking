using System.Text.Json;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using KRTBank.Application.Interfaces;

namespace KRTBank.Infrastructure.Publishers;

public class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _sns;
    private const int MaxRetries = 3;
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

    public async Task PublishAsync<T>(
        T message, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, JsonOptions);

        var request = new PublishRequest
        {
            TopicArn = Arn,
            Message = json
        };

        var delay = TimeSpan.FromSeconds(1);

        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await _sns.PublishAsync(request, cancellationToken);
                return;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < MaxRetries)
            {
                await Task.Delay(delay, cancellationToken);
                delay *= 2; // backoff exponencial
            }
        }

        throw new Exception("Não foi possível publicar a mensagem após várias tentativas.");
    }

    private bool IsTransient(Exception ex)
    {
        return ex is AmazonServiceException awsEx &&
               (awsEx.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                awsEx.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable);
    }


}