using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.ValueObjects;

namespace Pqrsdf.Infrastructure.Persistence.Configurations;

public sealed class PqrsdfTicketConfiguration : IEntityTypeConfiguration<PqrsdfTicket>
{
    public void Configure(EntityTypeBuilder<PqrsdfTicket> builder)
    {
        builder.ToTable("PqrsdfTickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RadicadoNumber)
            .HasConversion(
                r => r.Value,
                v => RadicadoNumber.Create(v).Value)
            .HasMaxLength(13)
            .IsRequired();

        builder.HasIndex(x => x.RadicadoNumber)
            .IsUnique();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne<DestinationArea>()
            .WithMany()
            .HasForeignKey(x => x.DestinationAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.IsAnonymous)
            .IsRequired();

        builder.OwnsOne(x => x.Applicant, applicantBuilder =>
        {
            applicantBuilder.Property(a => a.FullName)
                .HasColumnName("Applicant_FullName")
                .HasMaxLength(150);

            applicantBuilder.Property(a => a.IdentificationType)
                .HasColumnName("Applicant_IdType")
                .HasConversion<int>();

            applicantBuilder.Property(a => a.IdentificationNumber)
                .HasColumnName("Applicant_IdNumber")
                .HasMaxLength(20);

            applicantBuilder.Property(a => a.Email)
                .HasColumnName("Applicant_Email")
                .HasMaxLength(254);

            applicantBuilder.Property(a => a.PhoneNumber)
                .HasColumnName("Applicant_Phone")
                .HasMaxLength(20);
        });

        builder.Property(x => x.Subject)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.OwnsOne(x => x.DueDate, dueDateBuilder =>
        {
            dueDateBuilder.Property(d => d.Value)
                .HasColumnName("DueDate")
                .IsRequired();

            dueDateBuilder.Property(d => d.BusinessDaysCount)
                .HasColumnName("BusinessDaysCount")
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ResponseText)
            .HasMaxLength(4000);

        builder.Property(x => x.ResponseDateUtc);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Property(x => x.AssignedAtUtc);

        builder.Property(x => x.AssignmentNote)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.AssignedToUserId, x.Status });
        builder.HasIndex(x => x.Status);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.Ignore(x => x.DomainEvents);
    }
}
