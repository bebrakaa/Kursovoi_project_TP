using System.Security.Claims;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[Authorize(Roles = "Client")]
public class ClientController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentService _paymentService;
    private readonly IInsuranceServiceRepository _serviceRepository;
    private readonly IContractApplicationRepository _applicationRepository;
    private readonly IOperationHistoryRepository _historyRepository;
    private readonly IClientRepository _clientRepository;
    private readonly INotificationService _notificationService;

    public ClientController(
        IUserRepository userRepository,
        IContractRepository contractRepository,
        IPaymentRepository paymentRepository,
        IPaymentService paymentService,
        IInsuranceServiceRepository serviceRepository,
        IContractApplicationRepository applicationRepository,
        IOperationHistoryRepository historyRepository,
        IClientRepository clientRepository,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _contractRepository = contractRepository;
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
        _serviceRepository = serviceRepository;
        _applicationRepository = applicationRepository;
        _historyRepository = historyRepository;
        _clientRepository = clientRepository;
        _notificationService = notificationService;
    }

    // Управление профилем
    public async Task<IActionResult> Profile()
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

        // Получаем данные клиента, если они есть
        var clients = await _clientRepository.GetAllAsync();
        var client = clients.FirstOrDefault(c => c.Email == user.Email);
        ViewBag.Client = client;

        return View(user);
    }

    // Оплата договора
    [HttpGet]
    public async Task<IActionResult> PayContract(Guid contractId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
        {
            return NotFound();
        }

        ViewBag.Contract = contract;
        return View();
    }

    // История операций
    public async Task<IActionResult> History()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var contracts = await _contractRepository.GetAllAsync();
        var payments = await _paymentRepository.GetAllAsync();

        // Фильтруем по клиенту (здесь нужно будет связать User с Client)
        ViewBag.Contracts = contracts;
        ViewBag.Payments = payments;

        return View();
    }

    // Создание заявки на договор
    [HttpGet]
    public async Task<IActionResult> CreateApplication()
    {
        var services = await _serviceRepository.GetAllAsync();
        ViewBag.Services = services;
        return View(new Application.DTOs.ContractApplication.CreateApplicationDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApplication(Application.DTOs.ContractApplication.CreateApplicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            var services = await _serviceRepository.GetAllAsync();
            ViewBag.Services = services;
            return View(dto);
        }

        if (dto.DesiredEndDate <= dto.DesiredStartDate)
        {
            ModelState.AddModelError("DesiredEndDate", "Дата окончания должна быть позже даты начала");
            var services = await _serviceRepository.GetAllAsync();
            ViewBag.Services = services;
            return View(dto);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // Получаем клиента по userId (упрощенно, в реальности нужна связь User -> Client)
        var clients = await _clientRepository.GetAllAsync();
        var client = clients.FirstOrDefault(); // Временное решение

        if (client == null)
        {
            ModelState.AddModelError("", "Клиент не найден. Обратитесь к администратору.");
            var services = await _serviceRepository.GetAllAsync();
            ViewBag.Services = services;
            return View(dto);
        }

        try
        {
            var application = new Domain.Entities.ContractApplication(
                client.Id,
                dto.ServiceId,
                dto.DesiredStartDate,
                dto.DesiredEndDate,
                dto.DesiredPremium,
                dto.Notes);

            await _applicationRepository.AddAsync(application);
            await _applicationRepository.SaveChangesAsync();

            // Записываем в историю
            var history = new Domain.Entities.OperationHistory(
                userId,
                "CreateApplication",
                $"Создана заявка на договор: {dto.DesiredStartDate:dd.MM.yyyy} - {dto.DesiredEndDate:dd.MM.yyyy}",
                application.Id,
                "ContractApplication");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();

            // Отправляем уведомление о создании заявки
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _notificationService.SendAsync(
                    user.Email,
                    "Заявка на договор страхования создана",
                    $"Уважаемый(ая) {client.FullName ?? "Клиент"}!\n\nВаша заявка на договор страхования успешно создана и отправлена на рассмотрение агенту.\n\nМы свяжемся с вами в ближайшее время.");
            }

            TempData["SuccessMessage"] = "Заявка успешно создана";
            return RedirectToAction("MyApplications");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при создании заявки: {ex.Message}");
            var services = await _serviceRepository.GetAllAsync();
            ViewBag.Services = services;
            return View(dto);
        }
    }

    // Мои заявки
    public async Task<IActionResult> MyApplications()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        // Получаем клиента по userId (упрощенно)
        var clients = await _clientRepository.GetAllAsync();
        var client = clients.FirstOrDefault();

        if (client == null)
        {
            ViewBag.Applications = new List<Domain.Entities.ContractApplication>();
            return View();
        }

        var applications = await _applicationRepository.GetByClientIdAsync(client.Id);
        return View(applications);
    }

    // Обновление профиля
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(Application.DTOs.User.UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
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
            return View("Profile", user);
        }

        var userIdClaim2 = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim2 == null || !Guid.TryParse(userIdClaim2, out var userId2))
        {
            return RedirectToAction("Login", "Account");
        }

        var user2 = await _userRepository.GetByIdAsync(userId2);
        if (user2 == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            if (!string.Equals(user2.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingEmail != null && existingEmail.Id != user2.Id)
                {
                    ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                    return View("Profile", user2);
                }

                user2.UpdateEmail(dto.Email);
            }

            await _userRepository.UpdateAsync(user2);
            await _userRepository.SaveChangesAsync();

            // Обновляем данные клиента, если они есть
            var clients = await _clientRepository.GetAllAsync();
            var client = clients.FirstOrDefault(c => c.Email == user2.Email);
            if (client != null)
            {
                // Обновляем email если изменился
                if (!string.Equals(client.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    client.UpdateContact(client.FullName, dto.Email, client.Phone);
                }
                
                // Обновляем остальные данные
                if (!string.IsNullOrWhiteSpace(dto.FullName) || !string.IsNullOrWhiteSpace(dto.Phone) || !string.IsNullOrWhiteSpace(dto.Passport))
                {
                    client.UpdateContact(
                        dto.FullName ?? client.FullName,
                        dto.Email ?? client.Email,
                        dto.Phone ?? client.Phone);
                    
                    if (!string.IsNullOrWhiteSpace(dto.Passport))
                    {
                        client.SetPassport(dto.Passport);
                    }
                    
                    await _clientRepository.UpdateAsync(client);
                    await _clientRepository.SaveChangesAsync();
                }
            }
            else if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                // Создаем клиента, если его нет, но есть данные
                client = new Domain.Entities.Client(
                    dto.FullName,
                    dto.Email,
                    dto.Phone,
                    dto.Passport);
                await _clientRepository.AddAsync(client);
                await _clientRepository.SaveChangesAsync();
            }

            // Записываем в историю
            var history = new Domain.Entities.OperationHistory(
                userId2,
                "UpdateProfile",
                "Обновлен профиль пользователя",
                user2.Id,
                "User");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();

            // Обновляем ViewBag для отображения обновленных данных
            var updatedClients = await _clientRepository.GetAllAsync();
            var updatedClient = updatedClients.FirstOrDefault(c => c.Email == user2.Email);
            ViewBag.Client = updatedClient;

            TempData["SuccessMessage"] = "Профиль успешно обновлен";
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка: {ex.Message}");
            return View("Profile", user2);
        }
    }

    // Оплата договора (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(Guid contractId)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
        {
            return NotFound();
        }

        try
        {
            var paymentDto = new Application.DTOs.Payment.InitiatePaymentDto
            {
                ContractId = contractId,
                Amount = contract.Premium.Amount
            };

            var result = await _paymentService.InitiatePaymentAsync(paymentDto);

            // Записываем в историю
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                var history = new Domain.Entities.OperationHistory(
                    userId,
                    "Payment",
                    $"Оплата договора: {contract.Number ?? contract.Id.ToString()}",
                    contractId,
                    "Payment");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();
            }

            // Отправляем уведомление об успешной оплате
            if (result.Success && contract.Client != null && !string.IsNullOrEmpty(contract.Client.Email))
            {
                await _notificationService.SendAsync(
                    contract.Client.Email,
                    $"Договор {contract.Number ?? contract.Id.ToString()} успешно оплачен",
                    $"Уважаемый(ая) {contract.Client.FullName ?? "Клиент"}!\n\nВаш договор страхования {contract.Number ?? contract.Id.ToString()} успешно оплачен.\nСумма оплаты: {contract.Premium.Amount} {contract.Premium.Currency}\n\nСпасибо за использование наших услуг!");
            }

            TempData["SuccessMessage"] = "Договор успешно оплачен!";
            return RedirectToAction("History");
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка при оплате: {ex.Message}";
            ViewBag.Contract = contract;
            return View();
        }
    }
}

