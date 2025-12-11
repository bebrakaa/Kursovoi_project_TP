using System;

namespace InsuranceAgency.Application.DTOs
{
    public class ContractDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;

        public Guid ClientId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid AgentId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal PremiumAmount { get; set; }
        public string PremiumCurrency { get; set; } = "RUB";

        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
