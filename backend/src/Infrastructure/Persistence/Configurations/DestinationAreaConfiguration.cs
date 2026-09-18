using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pqrsdf.Domain.Entities;

namespace Pqrsdf.Infrastructure.Persistence.Configurations;

public sealed class DestinationAreaConfiguration : IEntityTypeConfiguration<DestinationArea>
{
    public void Configure(EntityTypeBuilder<DestinationArea> builder)
    {
        builder.ToTable("DestinationAreas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.Ignore(x => x.DomainEvents);

        // Seed initial official institutional areas
        builder.HasData(
            CreateSeedArea(new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "Atención al Ciudadano", "ATC"),
            CreateSeedArea(new Guid("4fa85f64-5717-4562-b3fc-2c963f66afa6"), "Oficina Jurídica", "JUR"),
            CreateSeedArea(new Guid("5fa85f64-5717-4562-b3fc-2c963f66afa6"), "Gestión Financiera", "FIN"),
            CreateSeedArea(new Guid("6fa85f64-5717-4562-b3fc-2c963f66afa6"), "Control Interno", "CIN"),
            CreateSeedArea(new Guid("7fa85f64-5717-4562-b3fc-2c963f66afa6"), "Dirección General", "DIR")
        );
    }

    private static object CreateSeedArea(Guid id, string name, string code)
    {
        return new
        {
            Id = id,
            Name = name,
            Code = code,
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = (DateTime?)null
        };
    }
}
