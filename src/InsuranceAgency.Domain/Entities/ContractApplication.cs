using System;
using InsuranceAgency.Domain.Enums;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// Заявка клиента на создание страхового договора
    /// </summary>
    public class ContractApplication
    {
        public Guid Id { get; private set; }
        public Guid ClientId { get; private set; }
        public Client? Client { get; private set; }
        public Guid ServiceId { get; private set; }
        public InsuranceService? Service { get; private set; }
        public DateTime DesiredStartDate { get; private set; }
        public DateTime DesiredEndDate { get; private set; }
        public decimal DesiredPremium { get; private set; }
        public string? Notes { get; private set; }
        public ApplicationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public Guid? ProcessedByAgentId { get; private set; }
        public Agent? ProcessedByAgent { get; private set; }

        protected ContractApplication() { }

        public ContractApplication(
            Guid clientId,
            Guid serviceId,
            DateTime desiredStartDate,
            DateTime desiredEndDate,
            decimal desiredPremium,
            string? notes = null)
        {
            if (clientId == Guid.Empty) throw new ArgumentException("ClientId is required");
            if (serviceId == Guid.Empty) throw new ArgumentException("ServiceId is required");
            if (desiredEndDate < desiredStartDate) throw new ArgumentException("EndDate must be after StartDate");
            if (desiredPremium <= 0) throw new ArgumentException("Premium must be positive");

            Id = Guid.NewGuid();
            ClientId = clientId;
            ServiceId = serviceId;
            DesiredStartDate = desiredStartDate;
            DesiredEndDate = desiredEndDate;
            DesiredPremium = desiredPremium;
            Notes = notes;
            Status = ApplicationStatus.Pending;
            CreatedAt = UpdatedAt = DateTime.UtcNow;
        }

        public void Approve(Guid agentId)
        {
            if (Status != ApplicationStatus.Pending)
                throw new InvalidOperationException($"Cannot approve application in status {Status}");

            Status = ApplicationStatus.Approved;
            ProcessedByAgentId = agentId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(string? reason = null)
        {
            if (Status != ApplicationStatus.Pending)
                throw new InvalidOperationException($"Cannot reject application in status {Status}");

            Status = ApplicationStatus.Rejected;
            Notes = string.IsNullOrWhiteSpace(Notes) ? $"Rejected: {reason}" : $"{Notes} | Rejected: {reason}";
            UpdatedAt = DateTime.UtcNow;
        }

        public void Process(Guid agentId)
        {
            if (Status != ApplicationStatus.Approved)
                throw new InvalidOperationException($"Cannot process application in status {Status}");

            Status = ApplicationStatus.Processed;
            ProcessedByAgentId = agentId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

