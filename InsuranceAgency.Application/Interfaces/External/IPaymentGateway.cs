using System.Threading.Tasks;

namespace InsuranceAgency.Application.Interfaces.External
{
    public interface IPaymentGateway
    {
        Task<(bool success, string? transactionId, string? error)> ProcessPaymentAsync(
            decimal amount,
            string currency,
            string idempotencyKey);
    }
}
