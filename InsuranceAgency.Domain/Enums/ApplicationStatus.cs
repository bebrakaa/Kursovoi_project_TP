namespace InsuranceAgency.Domain.Enums
{
    public enum ApplicationStatus
    {
        Pending,    // Ожидает обработки
        Approved,   // Одобрена
        Rejected,   // Отклонена
        Processed   // Обработана (договор создан)
    }
}

