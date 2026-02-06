using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using KRTBank.Application.Interfaces;
using KRTBank.Infrastructure.Cache;
using KRTBank.Infrastructure.Options;
using KRTBank.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KRTBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<RedisOptions>()
            .Bind(configuration.GetRequiredSection(RedisOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "Endpoint is required")
            .Validate(o => o.TtlInDays > 0, "TtlInDays must be greater than zero")
            .ValidateOnStart();
        
        services.AddAWSService<IAmazonDynamoDB>();

        services.AddSingleton<IDynamoDBContext, DynamoDBContext>(); 

        services.AddSingleton<IAccountRepository, AccountRepository>();
        
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}