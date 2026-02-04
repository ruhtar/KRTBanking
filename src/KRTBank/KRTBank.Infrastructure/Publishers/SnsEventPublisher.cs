using System.Text.Json;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using KRTBank.Application.Interfaces;
using KRTBank.Infrastructure.Options;
using KRTBank.Infrastructure.Secrets;
using Microsoft.Extensions.Options;

namespace KRTBank.Infrastructure.Publishers;

public class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _sns;
    private const int MaxRetries = 3;
    private readonly string _arn;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    public SnsEventPublisher(IAmazonSimpleNotificationService sns, IOptions<SnsOptions> options, AwsSecretProvider secretProvider)
    {
        _sns = sns;

        var secretName = options.Value.SecretName;
        var secret = secretProvider.GetSecretAsync<SnsSecret>(secretName)
            .GetAwaiter().GetResult();

        _arn = secret.TopicArn;
    }

    public async Task PublishAsync<T>(
        T message, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, JsonOptions);

        var request = new PublishRequest
        {
            TopicArn = _arn,
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