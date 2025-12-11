namespace InsuranceAgency.Domain.Enums
{
    public enum PaymentStatus
    {
        Created,
        Processing,
        Confirmed,
        Failed,
        Refunded,
        Chargeback,
        Timeout
    }
}
