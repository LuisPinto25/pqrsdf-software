using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Enums;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasConversion(
                e => e.Value,
                v => Email.Create(v).Value)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasConversion(
                p => p.Value,
                v => PasswordHash.Create(v).Value)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.LastLoginAtUtc);

        builder.Ignore(x => x.DomainEvents);

        // Pre-seeded development accounts
        // Raw passwords:
        // admin@pqrsdf.gov.co -> Admin123*
        // funcionario@pqrsdf.gov.co -> Funcionario123*
        builder.HasData(
            CreateSeedUser(
                new Guid("B81B94C8-E0C7-4187-8A33-89AC54395E01"),
                "admin@pqrsdf.gov.co",
                "$2a$11$MJdcdRnVBhcwT5M/6j6VWedZXXxbf2S4xRTp5/JZKUheied8ihHO2",
                "Administrador del Sistema",
                UserRole.Administrador),
            CreateSeedUser(
                new Guid("E49D34F1-9BD7-40A5-926B-9548BE740F02"),
                "funcionario@pqrsdf.gov.co",
                "$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy",
                "Funcionario de PQRSDF",
                UserRole.Funcionario)
        );
    }

    private static object CreateSeedUser(
        Guid id,
        string email,
        string passwordHash,
        string fullName,
        UserRole role)
    {
        return new
        {
            Id = id,
            Email = Email.Create(email).Value,
            PasswordHash = PasswordHash.Create(passwordHash).Value,
            FullName = fullName,
            Role = role,
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            LastLoginAtUtc = (DateTime?)null
        };
    }
}
