using System;

namespace InsuranceAgency.Domain.Entities
{
    public class Agent
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string? EmployeeNumber { get; private set; }

        protected Agent() { }

        public Agent(string fullName, string email, string? employeeNumber = null)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName required");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required");
            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            EmployeeNumber = employeeNumber;
        }
    }
}
