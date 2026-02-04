using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using KRTBank.Domain.Interfaces;
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

        // Mensageria / Eventos (se criar depois)
        // services.AddScoped<IAccountEventPublisher, AccountEventPublisher>();

        return services;
    }
}