using System;
using InsuranceAgency.Domain.Enums;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// Верификация документов клиента
    /// </summary>
    public class DocumentVerification
    {
        public Guid Id { get; private set; }
        public Guid ClientId { get; private set; }
        public Client? Client { get; private set; }
        public Guid? VerifiedByAgentId { get; private set; } // Nullable для заявок от клиента
        public Agent? VerifiedByAgent { get; private set; }
        public VerificationStatus Status { get; private set; }
        public string? DocumentType { get; private set; } // "Passport", "DriverLicense", etc.
        public string? DocumentNumber { get; private set; }
        public string? Notes { get; private set; }
        public DateTime VerifiedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected DocumentVerification() { }

        public DocumentVerification(
            Guid clientId,
            Guid? verifiedByAgentId = null,
            string? documentType = null,
            string? documentNumber = null,
            string? notes = null)
        {
            if (clientId == Guid.Empty) throw new ArgumentException("ClientId is required");

            Id = Guid.NewGuid();
            ClientId = clientId;
            VerifiedByAgentId = verifiedByAgentId;
            DocumentType = documentType;
            DocumentNumber = documentNumber;
            Notes = notes;
            Status = VerificationStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            VerifiedAt = DateTime.UtcNow;
        }

        // Метод для назначения агента при обработке заявки
        public void AssignAgent(Guid agentId)
        {
            if (agentId == Guid.Empty) throw new ArgumentException("AgentId is required");
            VerifiedByAgentId = agentId;
        }

        public void Approve(Guid agentId, string? notes = null)
        {
            if (agentId == Guid.Empty) throw new ArgumentException("AgentId is required");
            if (VerifiedByAgentId == null)
            {
                VerifiedByAgentId = agentId;
            }
            Status = VerificationStatus.Approved;
            Notes = string.IsNullOrWhiteSpace(Notes) ? notes : $"{Notes} | {notes}";
            VerifiedAt = DateTime.UtcNow;
        }

        public void Reject(Guid agentId, string reason)
        {
            if (agentId == Guid.Empty) throw new ArgumentException("AgentId is required");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required");

            if (VerifiedByAgentId == null)
            {
                VerifiedByAgentId = agentId;
            }
            Status = VerificationStatus.Rejected;
            Notes = string.IsNullOrWhiteSpace(Notes) ? $"Rejected: {reason}" : $"{Notes} | Rejected: {reason}";
            VerifiedAt = DateTime.UtcNow;
        }
    }
}

