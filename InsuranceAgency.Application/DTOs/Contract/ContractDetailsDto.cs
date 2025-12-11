using System;

namespace InsuranceAgency.Application.DTOs.Contract
{
    public class ContractDetailsDto
    {
        public Guid Id { get; set; }
        public string? ContractNumber { get; set; }
        public string Status { get; set; } = null!;
        public decimal Premium { get; set; }
        public bool IsPaid { get; set; }
        public string? Notes { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string ClientFullName { get; set; } = null!;
        public string InsuranceServiceTitle { get; set; } = null!;
    }
}
