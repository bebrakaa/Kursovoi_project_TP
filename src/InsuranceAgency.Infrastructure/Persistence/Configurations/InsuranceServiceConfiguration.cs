using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAgency.Infrastructure.Persistence.Configurations;

public class InsuranceServiceConfiguration : IEntityTypeConfiguration<InsuranceService>
{
    public void Configure(EntityTypeBuilder<InsuranceService> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        // Конфигурация для Money value object
        builder.OwnsOne(s => s.DefaultPremium, premium =>
        {
            premium.Property(p => p.Amount)
                .HasColumnName("DefaultPremiumAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            premium.Property(p => p.Currency)
                .HasColumnName("DefaultPremiumCurrency")
                .HasMaxLength(10)
                .IsRequired();
        });
    }
}
