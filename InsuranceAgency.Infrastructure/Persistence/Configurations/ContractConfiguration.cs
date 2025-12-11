using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAgency.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Number)
            .HasMaxLength(100);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        // Конфигурация для Money value object
        builder.OwnsOne(c => c.Premium, premium =>
        {
            premium.Property(p => p.Amount)
                .HasColumnName("PremiumAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            premium.Property(p => p.Currency)
                .HasColumnName("PremiumCurrency")
                .HasMaxLength(10)
                .IsRequired();
        });

        builder.Property(c => c.IsPaid)
            .IsRequired();

        builder.Property(c => c.IsFlaggedProblem)
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Agent)
            .WithMany()
            .HasForeignKey(c => c.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Service)
            .WithMany()
            .HasForeignKey(c => c.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Payments)
            .WithOne(p => p.Contract)
            .HasForeignKey(p => p.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
