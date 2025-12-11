using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAgency.Infrastructure.Persistence.Configurations;

public class ContractApplicationConfiguration : IEntityTypeConfiguration<ContractApplication>
{
    public void Configure(EntityTypeBuilder<ContractApplication> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ClientId)
            .IsRequired();

        builder.Property(a => a.ServiceId)
            .IsRequired();

        builder.Property(a => a.DesiredStartDate)
            .IsRequired();

        builder.Property(a => a.DesiredEndDate)
            .IsRequired();

        builder.Property(a => a.DesiredPremium)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        builder.HasOne(a => a.Client)
            .WithMany()
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ProcessedByAgent)
            .WithMany()
            .HasForeignKey(a => a.ProcessedByAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.ClientId);
        builder.HasIndex(a => a.Status);
    }
}

