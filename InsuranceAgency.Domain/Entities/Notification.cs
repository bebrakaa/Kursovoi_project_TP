using System;

namespace InsuranceAgency.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public Guid RecipientId { get; private set; }
        public string Channel { get; private set; } = "email";
        public string Subject { get; private set; } = null!;
        public string Body { get; private set; } = null!;
        public bool Sent { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? SentAt { get; private set; }

        protected Notification() { }

        public Notification(Guid recipientId, string subject, string body, string channel = "email")
        {
            if (recipientId == Guid.Empty) throw new ArgumentException("recipientId required");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("subject required");
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("body required");

            Id = Guid.NewGuid();
            RecipientId = recipientId;
            Subject = subject;
            Body = body;
            Channel = channel;
            Sent = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkSent() { Sent = true; SentAt = DateTime.UtcNow; }
    }
}
