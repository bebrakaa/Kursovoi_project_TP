using System.Threading.Tasks;
using InsuranceAgency.Application.Interfaces.Services;

namespace InsuranceAgency.Application.Services
{
    public class NotificationService : INotificationService
    {
        public Task SendAsync(string email, string subject, string body)
        {
            // Здесь будет логика отправки email/sms (реализация позже в Infrastructure)
            return Task.CompletedTask;
        }
    }
}
