using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAgency.Infrastructure.Persistence.Configurations;

public class DocumentVerificationConfiguration : IEntityTypeConfiguration<DocumentVerification>
{
    public void Configure(EntityTypeBuilder<DocumentVerification> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ClientId)
            .IsRequired();

        builder.Property(v => v.VerifiedByAgentId)
            .IsRequired(false); // Nullable для заявок от клиента

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.DocumentType)
            .HasMaxLength(100);

        builder.Property(v => v.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(v => v.VerifiedAt)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.HasOne(v => v.Client)
            .WithMany()
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.VerifiedByAgent)
            .WithMany()
            .HasForeignKey(v => v.VerifiedByAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.ClientId);
        builder.HasIndex(v => v.Status);
    }
}

