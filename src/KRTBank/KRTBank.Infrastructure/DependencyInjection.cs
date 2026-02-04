using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleNotificationService;
using KRTBank.Application.Interfaces;
using KRTBank.Domain.Interfaces;
using KRTBank.Infrastructure.Publishers;
using KRTBank.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace KRTBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddAWSService<IAmazonDynamoDB>();

        services.AddSingleton<IDynamoDBContext, DynamoDBContext>(); // TODO: porque singleton?

        services.AddScoped<IAccountRepository, AccountRepository>();

        services.AddAWSService<IAmazonSimpleNotificationService>();
        services.AddScoped<IEventPublisher, SnsEventPublisher>();

        return services;
    }
}