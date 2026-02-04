using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleNotificationService;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Interfaces;
using KRTBank.Infrastructure.Cache;
using KRTBank.Infrastructure.Publishers;
using KRTBank.Infrastructure.Repositories;
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
        
        var redisEndpoint = configuration["Redis:Endpoint"] ?? "localhost:6379";
        services.AddSingleton<ICacheService>(_ => new CacheService(redisEndpoint)); // Mantém a conexão Redis aberta e compartilhada entre requests. Redis é thread-safe.

        return services;
    }
}