using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pqrsdf.Application.Shared.Persistence;
using Pqrsdf.Infrastructure.Persistence;

namespace Pqrsdf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=PqrsdfDb;User Id=sa;Password=YourStrong@Passw0rd!;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<PqrsdfDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IDatabaseStatusChecker, DatabaseStatusChecker>();

        return services;
    }
}
