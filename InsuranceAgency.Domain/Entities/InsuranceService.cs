using System;
using InsuranceAgency.Domain.ValueObjects;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// Описание страховой услуги / продукта.
    /// В Infrastructure конфигурации ожидается свойство Name.
    /// </summary>
    public class InsuranceService
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;      // совпадает с Configuration.Name
        public string? Description { get; private set; }
        public Money DefaultPremium { get; private set; } = null!;

        protected InsuranceService() { }

        public InsuranceService(string name, Money defaultPremium, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required");
            Id = Guid.NewGuid();
            Name = name;
            DefaultPremium = defaultPremium ?? throw new ArgumentNullException(nameof(defaultPremium));
            Description = description;
        }

        public void UpdateDefaultPremium(Money premium)
        {
            if (premium.Amount <= 0) throw new ArgumentException("premium must be > 0");
            DefaultPremium = premium;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required");
            Name = name;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }
    }
}
