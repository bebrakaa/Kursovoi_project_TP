using System;

namespace InsuranceAgency.Application.DTOs
{
    public class RegisterContractDto
    {
        public Guid ContractId { get; set; }
        public string Number { get; set; } = string.Empty;
        public Guid AgentId { get; set; }
    }
}
