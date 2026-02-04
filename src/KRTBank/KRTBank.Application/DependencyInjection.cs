using KRTBank.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KRTBank.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}