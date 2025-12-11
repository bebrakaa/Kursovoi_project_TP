using System.Threading.Tasks;

namespace InsuranceAgency.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(string email, string subject, string body);
    }
}
