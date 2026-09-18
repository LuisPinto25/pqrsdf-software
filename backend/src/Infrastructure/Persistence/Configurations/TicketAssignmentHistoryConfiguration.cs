using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pqrsdf.Domain.Entities;

namespace Pqrsdf.Infrastructure.Persistence.Configurations;

public sealed class TicketAssignmentHistoryConfiguration : IEntityTypeConfiguration<TicketAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<TicketAssignmentHistory> builder)
    {
        builder.ToTable("TicketAssignmentHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.AssignedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Ticket)
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PreviousAssignedUser)
            .WithMany()
            .HasForeignKey(x => x.PreviousAssignedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(x => x.NewAssignedUser)
            .WithMany()
            .HasForeignKey(x => x.NewAssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedByUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TicketId, x.AssignedAtUtc });

        builder.Ignore(x => x.DomainEvents);
    }
}
