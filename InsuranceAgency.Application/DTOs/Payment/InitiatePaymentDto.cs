using System;

namespace InsuranceAgency.Application.DTOs.Payment
{
    public class InitiatePaymentDto
    {
        public Guid ContractId { get; set; }
        public decimal Amount { get; set; }
    }
}
