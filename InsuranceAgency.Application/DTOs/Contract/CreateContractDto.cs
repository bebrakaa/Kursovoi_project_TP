using System;

namespace InsuranceAgency.Application.DTOs
{
    public class CreateContractDto
    {
        public Guid ClientId { get; set; }
        public Guid ServiceId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal PremiumAmount { get; set; }
        public string PremiumCurrency { get; set; } = "RUB";

        public string? Notes { get; set; }
    }
}
