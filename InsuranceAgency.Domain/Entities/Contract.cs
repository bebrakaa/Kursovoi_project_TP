using System;
using System.Collections.Generic;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.Exceptions;
using InsuranceAgency.Domain.ValueObjects;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// Договор страхования.
    /// Навигационные свойства оставлены для EF: Client, Agent, Service, Payments.
    /// В домене — бизнес-методы для переходов состояний.
    /// </summary>
    public class Contract
    {
        public Guid Id { get; private set; }
        public string? Number { get; private set; }           // совпадает с Infrastructure ContractConfiguration.Number
        public Guid ClientId { get; private set; }
        public Client? Client { get; private set; }           // навигация для EF
        public Guid? AgentId { get; private set; }
        public Agent? Agent { get; private set; }             // навигация для EF
        public Guid ServiceId { get; private set; }
        public InsuranceService? Service { get; private set; } // навигация для EF

        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }
        public Money Premium { get; private set; } = null!;   // required

        public ContractStatus Status { get; private set; }
        public bool IsPaid { get; private set; }
        public bool IsFlaggedProblem { get; private set; }

        public string? Notes { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<Payment> _payments = new();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        protected Contract() { } // Для ORM

        public Contract(Guid clientId, Guid serviceId, DateOnly startDate, DateOnly endDate, Money premium, Guid? agentId = null, string? notes = null)
        {
            if (clientId == Guid.Empty) throw new ValidationException("ClientId is required");
            if (serviceId == Guid.Empty) throw new ValidationException("ServiceId is required");
            if (endDate < startDate) throw new ValidationException("EndDate must be after StartDate");
            if (premium == null) throw new ValidationException("Premium is required");
            if (premium.Amount <= 0) throw new ValidationException("Premium amount must be positive");

            Id = Guid.NewGuid();
            ClientId = clientId;
            ServiceId = serviceId;
            AgentId = agentId;
            StartDate = startDate;
            EndDate = endDate;
            Premium = premium;
            Status = ContractStatus.Draft;
            IsPaid = false;
            IsFlaggedProblem = false;
            Notes = notes;
            CreatedAt = UpdatedAt = DateTime.UtcNow;
        }

        // Присвоить номер и перевести в Registered
        public void Register(string number, Guid actorAgentId)
        {
            if (string.IsNullOrWhiteSpace(number)) throw new ValidationException("Contract number cannot be empty");
            if (Status != ContractStatus.Draft && Status != ContractStatus.Suspended)
                throw new DomainException($"Cannot register contract from state {Status}");

            Number = number;
            AgentId = actorAgentId;
            Status = ContractStatus.Registered;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPaid()
        {
            if (IsPaid) return;
            IsPaid = true;
            if (Status == ContractStatus.Registered || Status == ContractStatus.PendingPayment)
                Status = ContractStatus.Paid;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (Status != ContractStatus.Paid)
                throw new DomainException($"Cannot activate contract from state {Status}. Contract must be Paid.");

            Status = ContractStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkOverdue()
        {
            Status = ContractStatus.Overdue;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Suspend(string? reason = null)
        {
            Status = ContractStatus.Suspended;
            Notes = AppendNote(Notes, $"Suspended: {reason}");
            UpdatedAt = DateTime.UtcNow;
        }

        public void Resume()
        {
            if (Status != ContractStatus.Suspended)
                throw new DomainException("Contract is not suspended");

            Status = ContractStatus.Registered;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkProblematic(string reason)
        {
            IsFlaggedProblem = true;
            Status = ContractStatus.Problematic;
            Notes = AppendNote(Notes, $"Problem: {reason}");
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string? reason = null)
        {
            Status = ContractStatus.Cancelled;
            Notes = AppendNote(Notes, $"Cancelled: {reason}");
            UpdatedAt = DateTime.UtcNow;
        }

        public void Expire()
        {
            Status = ContractStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Renew(DateOnly newStart, DateOnly newEnd, Money newPremium)
        {
            if (newEnd < newStart) throw new ValidationException("newEnd must be after newStart");
            StartDate = newStart;
            EndDate = newEnd;
            Premium = newPremium;
            Status = ContractStatus.Registered;
            IsFlaggedProblem = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddPayment(Payment payment)
        {
            if (payment == null) throw new ValidationException("payment is required");
            if (!_payments.Contains(payment))
            {
                _payments.Add(payment);
            }
            UpdatedAt = DateTime.UtcNow;
        }

        private static string? AppendNote(string? existing, string? newNote)
        {
            if (string.IsNullOrWhiteSpace(newNote)) return existing;
            if (string.IsNullOrWhiteSpace(existing)) return newNote;
            return existing + " | " + newNote;
        }
    }
}
