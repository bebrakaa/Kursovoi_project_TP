using System;

namespace InsuranceAgency.Domain.Entities
{
    public class SystemProcess
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public DateTime StartedAt { get; private set; }
        public DateTime? FinishedAt { get; private set; }
        public string? Result { get; private set; }

        protected SystemProcess() { }

        public SystemProcess(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name required");
            Id = Guid.NewGuid();
            Name = name;
            StartedAt = DateTime.UtcNow;
        }

        public void Finish(string? result = null)
        {
            Result = result;
            FinishedAt = DateTime.UtcNow;
        }
    }
}
