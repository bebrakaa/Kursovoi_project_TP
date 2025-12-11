using System;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// Клиент (физическое лицо) — содержит паспорт (если нужно), телефон.
    /// Поля минимальны, навигация к договорам выполняется в Contract.
    /// </summary>
    public class Client
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string? Phone { get; private set; }
        public string? Passport { get; private set; }    // совпадает с конфигурацией (Passport)

        protected Client() { }

        public Client(string fullName, string email, string? phone = null, string? passport = null)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName required", nameof(fullName));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required", nameof(email));

            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            Phone = phone;
            Passport = passport;
        }

        public void UpdateContact(string fullName, string email, string? phone = null)
        {
            if (!string.IsNullOrWhiteSpace(fullName)) FullName = fullName;
            if (!string.IsNullOrWhiteSpace(email)) Email = email;
            Phone = phone;
        }

        public void SetPassport(string passport)
        {
            if (string.IsNullOrWhiteSpace(passport)) throw new ArgumentException("passport is required");
            Passport = passport;
        }
    }
}
