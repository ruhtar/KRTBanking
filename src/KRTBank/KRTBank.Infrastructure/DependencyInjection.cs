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
        services.AddAWSService<IAmazonDynamoDB>();

        services.AddSingleton<IDynamoDBContext, DynamoDBContext>(); 

        services.AddScoped<IAccountRepository, AccountRepository>();

        services.Configure<RedisOptions>(
            configuration.GetRequiredSection(RedisOptions.SectionName));
        
        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}