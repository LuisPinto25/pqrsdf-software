using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pqrsdf.Infrastructure.Persistence.Entities;

namespace Pqrsdf.Infrastructure.Persistence.Configurations;

public sealed class RadicadoSequenceConfiguration : IEntityTypeConfiguration<RadicadoSequence>
{
    public void Configure(EntityTypeBuilder<RadicadoSequence> builder)
    {
        builder.ToTable("RadicadoSequences");

        builder.HasKey(x => x.Year);

        builder.Property(x => x.Year)
            .ValueGeneratedNever();

        builder.Property(x => x.CurrentValue)
            .IsRequired();
    }
}
