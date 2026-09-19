using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pqrsdf.Domain.Entities;

namespace Pqrsdf.Infrastructure.Persistence.Configurations;

public sealed class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
    {
        builder.ToTable("TicketStatusHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreviousStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.NewStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Justification)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Ticket)
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TicketId, x.ChangedAtUtc });

        builder.Ignore(x => x.DomainEvents);
    }
}
