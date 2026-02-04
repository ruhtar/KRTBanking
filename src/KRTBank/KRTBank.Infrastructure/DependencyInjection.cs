using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SecretsManager;
using Amazon.SimpleNotificationService;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Interfaces;
using KRTBank.Infrastructure.Cache;
using KRTBank.Infrastructure.Options;
using KRTBank.Infrastructure.Publishers;
using KRTBank.Infrastructure.Repositories;
using KRTBank.Infrastructure.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KRTBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAWSService<IAmazonDynamoDB>();

        services.AddSingleton<IDynamoDBContext, DynamoDBContext>(); // TODO: porque singleton?

        services.AddScoped<IAccountRepository, AccountRepository>();

        services.AddAWSService<IAmazonSimpleNotificationService>();
        services.AddSingleton<IEventPublisher, SnsEventPublisher>();
        
        services.AddAWSService<IAmazonSecretsManager>();
        services.AddSingleton<AwsSecretProvider>();
        
        services.Configure<RedisOptions>(
            configuration.GetRequiredSection(RedisOptions.SectionName));

        services.Configure<SnsOptions>(
            configuration.GetRequiredSection(SnsOptions.SectionName));
        
        services.AddSingleton<ICacheService, CacheService>(); // Mantém a conexão Redis aberta e compartilhada entre requests. Redis é thread-safe.


        return services;
    }
}