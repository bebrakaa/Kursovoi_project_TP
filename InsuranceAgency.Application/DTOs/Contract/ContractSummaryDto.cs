using System;

namespace InsuranceAgency.Application.DTOs.Contract
{
    public class ContractSummaryDto
    {
        public Guid Id { get; set; }
        public string? ContractNumber { get; set; }
        public string Status { get; set; } = null!;
        public decimal Premium { get; set; }
    }
}
