using System.Security.Claims;
using InsuranceAgency.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DashboardController(
        IUserRepository userRepository,
        IContractRepository contractRepository,
        IClientRepository clientRepository,
        IPaymentRepository paymentRepository)
    {
        _userRepository = userRepository;
        _contractRepository = contractRepository;
        _clientRepository = clientRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.User = user;
        ViewBag.Role = user.Role.ToString();

        // Получаем статистику в зависимости от роли
        if (user.Role == Domain.Enums.UserRole.Administrator || user.Role == Domain.Enums.UserRole.Agent)
        {
            var allContracts = await _contractRepository.GetAllAsync();
            var allClients = await _clientRepository.GetAllAsync();
            var allPayments = await _paymentRepository.GetAllAsync();

            ViewBag.TotalContracts = allContracts.Count();
            ViewBag.TotalClients = allClients.Count();
            ViewBag.TotalPayments = allPayments.Count();
            ViewBag.ActiveContracts = allContracts.Count(c => c.Status == Domain.Enums.ContractStatus.Active);
        }
        else if (user.Role == Domain.Enums.UserRole.Client)
        {
            // Для клиента показываем только его данные
            // Это будет реализовано через связь User -> Client
            ViewBag.TotalContracts = 0;
            ViewBag.TotalPayments = 0;
        }

        return View();
    }
}

