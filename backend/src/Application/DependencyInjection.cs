using Microsoft.Extensions.DependencyInjection;

namespace Pqrsdf.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddScoped<Pqrsdf.Domain.Services.DueDateCalculator>();

        return services;
    }
}
