using System;

namespace InsuranceAgency.Domain.Entities
{
    /// <summary>
    /// История операций пользователя в системе
    /// </summary>
    public class OperationHistory
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string OperationType { get; private set; } = null!; // "CreateContract", "RegisterContract", "Payment", "UpdateProfile", etc.
        public string Description { get; private set; } = null!;
        public Guid? RelatedEntityId { get; private set; } // ID связанной сущности (договор, платеж и т.д.)
        public string? RelatedEntityType { get; private set; } // "Contract", "Payment", "Client", etc.
        public DateTime CreatedAt { get; private set; }

        protected OperationHistory() { }

        public OperationHistory(Guid userId, string operationType, string description, Guid? relatedEntityId = null, string? relatedEntityType = null)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId is required");
            if (string.IsNullOrWhiteSpace(operationType)) throw new ArgumentException("OperationType is required");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required");

            Id = Guid.NewGuid();
            UserId = userId;
            OperationType = operationType;
            Description = description;
            RelatedEntityId = relatedEntityId;
            RelatedEntityType = relatedEntityType;
            CreatedAt = DateTime.UtcNow;
        }
    }
}

