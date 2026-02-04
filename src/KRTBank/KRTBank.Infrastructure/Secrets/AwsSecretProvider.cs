using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace KRTBank.Infrastructure.Secrets;

public class AwsSecretProvider
{
    private readonly IAmazonSecretsManager _secrets;

    public AwsSecretProvider(IAmazonSecretsManager secrets)
    {
        _secrets = secrets;
    }

    public async Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default)
    {
        var response = await _secrets.GetSecretValueAsync(
            new GetSecretValueRequest { SecretId = secretName }, ct);

        if (response is null || string.IsNullOrWhiteSpace(response.SecretString))
        {
            throw new InvalidOperationException(
                $"Secret '{secretName}' was not found or is empty in AWS Secrets Manager.");
        }

        return JsonSerializer.Deserialize<T>(response.SecretString)
               ?? throw new InvalidOperationException(
                   $"Failed to deserialize secret '{secretName}' into type {typeof(T).Name}.");
    }
}