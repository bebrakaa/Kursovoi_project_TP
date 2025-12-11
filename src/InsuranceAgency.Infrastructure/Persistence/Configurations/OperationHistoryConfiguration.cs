using InsuranceAgency.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAgency.Infrastructure.Persistence.Configurations;

public class OperationHistoryConfiguration : IEntityTypeConfiguration<OperationHistory>
{
    public void Configure(EntityTypeBuilder<OperationHistory> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.OperationType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.CreatedAt);
    }
}

