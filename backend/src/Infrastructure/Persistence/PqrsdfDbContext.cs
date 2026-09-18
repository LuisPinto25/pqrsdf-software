using Microsoft.EntityFrameworkCore;

namespace Pqrsdf.Infrastructure.Persistence;

/// <summary>
/// Single unified Entity Framework Core DbContext for the PQRSDF application,
/// complying with Constitution Principle II.
/// </summary>
public class PqrsdfDbContext : DbContext
{
    public PqrsdfDbContext(DbContextOptions<PqrsdfDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PqrsdfDbContext).Assembly);
    }
}
