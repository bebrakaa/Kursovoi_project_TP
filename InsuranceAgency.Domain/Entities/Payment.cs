using System;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.Exceptions;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// Платёж, привязанный к договору.
    /// Имеет навигацию Contract для EF.
    /// </summary>
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid ContractId { get; private set; }
        public Contract? Contract { get; private set; } // навигация

        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "RUB";
        public PaymentStatus Status { get; private set; }

        public string? PspTransactionId { get; private set; }
        public string? IdempotencyKey { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public int Attempts { get; private set; }

        protected Payment() { }

        public Payment(Guid contractId, decimal amount, string currency = "RUB", string? idempotencyKey = null)
        {
            if (contractId == Guid.Empty) throw new ValidationException("ContractId required");
            if (amount <= 0) throw new ValidationException("Amount must be positive");
            if (string.IsNullOrWhiteSpace(currency)) throw new ValidationException("Currency required");

            Id = Guid.NewGuid();
            ContractId = contractId;
            Amount = amount;
            Currency = currency;
            Status = PaymentStatus.Created;
            IdempotencyKey = idempotencyKey;
            CreatedAt = UpdatedAt = DateTime.UtcNow;
            Attempts = 0;
        }

        public void MarkProcessing()
        {
            if (Status != PaymentStatus.Created && Status != PaymentStatus.Failed && Status != PaymentStatus.Timeout)
                throw new DomainException($"Cannot mark processing from state {Status}");

            Status = PaymentStatus.Processing;
            Attempts++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkConfirmed(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) throw new ValidationException("transactionId required");
            Status = PaymentStatus.Confirmed;
            PspTransactionId = transactionId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkFailed(string? reason = null)
        {
            Status = PaymentStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkRefunded()
        {
            if (Status != PaymentStatus.Confirmed) throw new DomainException("Only confirmed payments can be refunded");
            Status = PaymentStatus.Refunded;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkChargeback()
        {
            Status = PaymentStatus.Chargeback;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkTimeout()
        {
            Status = PaymentStatus.Timeout;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
