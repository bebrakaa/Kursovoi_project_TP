using InsuranceAgency.Application.Interfaces.External;

namespace InsuranceAgency.Infrastructure.External.PaymentGateway;

/// <summary>
/// Заглушка платежной системы для тестирования и разработки.
/// Не выполняет реальных платежей, всегда возвращает успешный результат.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    public Task<(bool success, string? transactionId, string? error)>
        ProcessPaymentAsync(decimal amount, string currency, string idempotencyKey)
    {
        // Заглушка: всегда успешный платёж с мнимым transactionId
        // В реальной системе здесь был бы вызов внешнего API платежной системы
        var mockTransactionId = $"MOCK-{Guid.NewGuid():N}";
        return Task.FromResult<(bool success, string? transactionId, string? error)>(
            (true, mockTransactionId, null));
    }
}
