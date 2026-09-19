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
        services.AddScoped<Pqrsdf.Domain.Services.IRadicadoSequenceGenerator, Services.RadicadoSequenceGenerator>();
        services.AddSingleton<Pqrsdf.Domain.Services.IColombianHolidayService, Services.ColombianHolidayService>();
        services.AddScoped<Pqrsdf.Domain.Repositories.IDestinationAreaRepository, Persistence.Repositories.DestinationAreaRepository>();
        services.AddScoped<Pqrsdf.Domain.Repositories.IPqrsdfTicketRepository, Persistence.Repositories.PqrsdfTicketRepository>();
        services.AddScoped<Pqrsdf.Domain.Repositories.IUserRepository, Persistence.Repositories.UserRepository>();
        services.AddScoped<Pqrsdf.Domain.Repositories.ITicketAssignmentHistoryRepository, Persistence.Repositories.TicketAssignmentHistoryRepository>();
        services.AddScoped<Pqrsdf.Domain.Repositories.ITicketStatusHistoryRepository, Persistence.Repositories.TicketStatusHistoryRepository>();
        services.AddSingleton<Pqrsdf.Application.Common.Interfaces.IPasswordHasher, Security.BCryptPasswordHasher>();
        services.AddScoped<Pqrsdf.Application.Common.Interfaces.IJwtTokenGenerator, Security.JwtTokenGenerator>();

        return services;
    }
}
