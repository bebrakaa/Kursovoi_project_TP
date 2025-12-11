namespace InsuranceAgency.Application.DTOs.Payment
{
    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? TransactionId { get; set; }
    }
}
