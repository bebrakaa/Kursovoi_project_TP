using System;

namespace InsuranceAgency.Domain.ValueObjects
{
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "RUB")
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative");
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency required");
            Amount = decimal.Round(amount, 2);
            Currency = currency;
        }

        public Money Add(Money other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other.Currency != Currency) throw new InvalidOperationException("Currency mismatch");
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other.Currency != Currency) throw new InvalidOperationException("Currency mismatch");
            var result = Amount - other.Amount;
            if (result < 0) throw new InvalidOperationException("Resulting amount cannot be negative");
            return new Money(result, Currency);
        }

        public override bool Equals(object? obj) => Equals(obj as Money);
        public bool Equals(Money? other) => other is not null && Amount == other.Amount && Currency == other.Currency;
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        public override string ToString() => $"{Amount:0.00} {Currency}";
    }
}
