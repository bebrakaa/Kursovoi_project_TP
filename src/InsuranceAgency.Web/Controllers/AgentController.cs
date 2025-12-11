using System.Security.Claims;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.Exceptions;
using InsuranceAgency.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[Authorize(Roles = "Agent,Administrator")]
public class AgentController : Controller
{
    private readonly IClientRepository _clientRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IContractService _contractService;
    private readonly IInsuranceServiceRepository _serviceRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDocumentVerificationRepository _verificationRepository;
    private readonly IOperationHistoryRepository _historyRepository;
    private readonly IContractApplicationRepository _applicationRepository;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IClientRepository clientRepository,
        IContractRepository contractRepository,
        IContractService contractService,
        IInsuranceServiceRepository serviceRepository,
        IAgentRepository agentRepository,
        IUserRepository userRepository,
        IDocumentVerificationRepository verificationRepository,
        IOperationHistoryRepository historyRepository,
        IContractApplicationRepository applicationRepository,
        ILogger<AgentController> logger)
    {
        _clientRepository = clientRepository;
        _contractRepository = contractRepository;
        _contractService = contractService;
        _serviceRepository = serviceRepository;
        _agentRepository = agentRepository;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _historyRepository = historyRepository;
        _applicationRepository = applicationRepository;
        _logger = logger;
    }

    // Справочник клиентов
    public async Task<IActionResult> Clients(string search = "")
    {
        var clients = await _clientRepository.GetAllAsync();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            clients = clients.Where(c => 
                c.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (c.Phone != null && c.Phone.Contains(search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        ViewBag.Search = search;
        return View(clients);
    }

    // Создание договора
    [HttpGet]
    public async Task<IActionResult> CreateContract()
    {
        var clients = await _clientRepository.GetAllAsync();
        var services = await _serviceRepository.GetAllAsync();
        var agents = await _agentRepository.GetAllAsync();

        ViewBag.Clients = clients;
        ViewBag.Services = services;
        ViewBag.Agents = agents;

        return View(new Application.DTOs.Contract.CreateContractFormDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateContract(Application.DTOs.Contract.CreateContractFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            var clients = await _clientRepository.GetAllAsync();
            var services = await _serviceRepository.GetAllAsync();
            var agents = await _agentRepository.GetAllAsync();

            ViewBag.Clients = clients;
            ViewBag.Services = services;
            ViewBag.Agents = agents;
            return View(dto);
        }

        if (dto.EndDate <= dto.StartDate)
        {
            ModelState.AddModelError("EndDate", "Дата окончания должна быть позже даты начала");
            var clients = await _clientRepository.GetAllAsync();
            var services = await _serviceRepository.GetAllAsync();
            var agents = await _agentRepository.GetAllAsync();

            ViewBag.Clients = clients;
            ViewBag.Services = services;
            ViewBag.Agents = agents;
            return View(dto);
        }

        try
        {
            var service = await _serviceRepository.GetByIdAsync(dto.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Услуга не найдена");
                var clients = await _clientRepository.GetAllAsync();
                var services = await _serviceRepository.GetAllAsync();
                var agents = await _agentRepository.GetAllAsync();

                ViewBag.Clients = clients;
                ViewBag.Services = services;
                ViewBag.Agents = agents;
                return View(dto);
            }

            var premium = new Money(dto.PremiumAmount, service.DefaultPremium.Currency);
            var contract = new Contract(dto.ClientId, dto.ServiceId, dto.StartDate, dto.EndDate, premium, dto.AgentId, dto.Notes);

            await _contractRepository.AddAsync(contract);
            await _contractRepository.SaveChangesAsync();

            // Записываем в историю
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                var history = new OperationHistory(
                    userId,
                    "CreateContract",
                    $"Создан договор: {dto.StartDate:dd.MM.yyyy} - {dto.EndDate:dd.MM.yyyy}",
                    contract.Id,
                    "Contract");
                await _historyRepository.AddAsync(history);
                await _historyRepository.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Договор успешно создан";
            return RedirectToAction("Contracts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании договора для клиента {ClientId}", dto.ClientId);
            ModelState.AddModelError("", $"Ошибка при создании договора: {ex.Message}");
            var clients = await _clientRepository.GetAllAsync();
            var services = await _serviceRepository.GetAllAsync();
            var agents = await _agentRepository.GetAllAsync();

            ViewBag.Clients = clients;
            ViewBag.Services = services;
            ViewBag.Agents = agents;
            return View(dto);
        }
    }

    // Регистрация договора
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterContract(Guid id, string number)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        if (contract == null)
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        // Получаем агента по email пользователя или создаем временного
        var agent = await _agentRepository.GetByEmailAsync(user.Email);
        if (agent == null)
        {
            var agents = await _agentRepository.GetAllAsync();
            agent = agents.FirstOrDefault();
            if (agent == null)
            {
                // Создаем временного агента на основе пользователя
                agent = new Agent(user.Username, user.Email);
                await _agentRepository.AddAsync(agent);
                await _agentRepository.SaveChangesAsync();
            }
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            TempData["ErrorMessage"] = "Номер договора обязателен";
            return RedirectToAction("Contracts");
        }

        try
        {
            contract.Register(number, agent.Id);
            await _contractRepository.UpdateAsync(contract);
            await _contractRepository.SaveChangesAsync();

            // Записываем в историю
            var history = new OperationHistory(
                userId,
                "RegisterContract",
                $"Зарегистрирован договор: {number}",
                contract.Id,
                "Contract");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Договор успешно зарегистрирован с номером: {number}";
            return RedirectToAction("Contracts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации договора {ContractId}", id);
            TempData["ErrorMessage"] = $"Ошибка при регистрации договора: {ex.Message}";
            return RedirectToAction("Contracts");
        }
    }

    // Просмотр договоров
    public async Task<IActionResult> Contracts(string search = "", string status = "")
    {
        var contracts = await _contractRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            contracts = contracts.Where(c => 
                c.Number != null && c.Number.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ContractStatus>(status, out var statusEnum))
        {
            contracts = contracts.Where(c => c.Status == statusEnum).ToList();
        }

        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(contracts);
    }

    // Поиск договоров
    public async Task<IActionResult> SearchContracts(string query)
    {
        var contracts = await _contractRepository.GetAllAsync();
        
        if (!string.IsNullOrWhiteSpace(query))
        {
            contracts = contracts.Where(c => 
                (c.Number != null && c.Number.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (c.Client != null && c.Client.FullName.Contains(query, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        return View("Contracts", contracts);
    }

    // История операций клиента
    public async Task<IActionResult> ClientHistory(Guid clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            return NotFound();
        }

        var contracts = await _contractRepository.GetAllAsync();
        var clientContracts = contracts.Where(c => c.ClientId == clientId).ToList();
        var verifications = await _verificationRepository.GetByClientIdAsync(clientId);

        ViewBag.Client = client;
        ViewBag.Contracts = clientContracts;
        ViewBag.Verifications = verifications;

        return View();
    }

    // Добавление клиента
    [HttpGet]
    public IActionResult AddClient()
    {
        return View(new Application.DTOs.Client.CreateClientDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClient(Application.DTOs.Client.CreateClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var client = new Domain.Entities.Client(dto.FullName, dto.Email, dto.Phone, dto.Passport);
            await _clientRepository.AddAsync(client);
            await _clientRepository.SaveChangesAsync();

            // Записываем в историю
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                var history = new Domain.Entities.OperationHistory(
                    userId,
                    "CreateClient",
                    $"Создан клиент: {dto.FullName}",
                    client.Id,
                    "Client");
                await _historyRepository.AddAsync(history);
                await _historyRepository.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Клиент успешно добавлен";
            return RedirectToAction("Clients");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании клиента: {FullName}", dto.FullName);
            ModelState.AddModelError("", $"Ошибка при создании клиента: {ex.Message}");
            return View(dto);
        }
    }

    // Редактирование клиента
    [HttpGet]
    public async Task<IActionResult> EditClient(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound();
        }

        return View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(Guid id, Application.DTOs.Client.UpdateClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        var clientToUpdate = await _clientRepository.GetByIdAsync(id);
        if (clientToUpdate == null)
        {
            return NotFound();
        }

        try
        {
            clientToUpdate.UpdateContact(dto.FullName, dto.Email, dto.Phone);
            if (!string.IsNullOrWhiteSpace(dto.Passport))
            {
                clientToUpdate.SetPassport(dto.Passport);
            }

            await _clientRepository.UpdateAsync(clientToUpdate);
            await _clientRepository.SaveChangesAsync();

            // Записываем в историю
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                var history = new Domain.Entities.OperationHistory(
                    userId,
                    "UpdateClient",
                    $"Обновлен клиент: {dto.FullName}",
                    clientToUpdate.Id,
                    "Client");
                await _historyRepository.AddAsync(history);
                await _historyRepository.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Клиент успешно обновлен";
            return RedirectToAction("Clients");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении клиента {ClientId}", id);
            ModelState.AddModelError("", $"Ошибка: {ex.Message}");
            return View(clientToUpdate);
        }
    }

    // Удаление клиента
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClient(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound();
        }

        try
        {
            await _clientRepository.DeleteAsync(client);
            await _clientRepository.SaveChangesAsync();

            // Записываем в историю
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                var history = new Domain.Entities.OperationHistory(
                    userId,
                    "DeleteClient",
                    $"Удален клиент: {client.FullName}",
                    client.Id,
                    "Client");
                await _historyRepository.AddAsync(history);
                await _historyRepository.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Клиент {client.FullName} успешно удален";
            return RedirectToAction("Clients");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении клиента {ClientId}", id);
            TempData["ErrorMessage"] = $"Ошибка при удалении клиента: {ex.Message}";
            return RedirectToAction("Clients");
        }
    }

    // Верификация документов
    [HttpGet]
    public async Task<IActionResult> VerifyDocuments(Guid clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            return NotFound();
        }

        var verifications = await _verificationRepository.GetByClientIdAsync(clientId);
        ViewBag.Client = client;
        ViewBag.Verifications = verifications;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyDocuments(Guid clientId, string documentType, string documentNumber, string? notes)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        documentType = documentType?.Trim() ?? string.Empty;
        documentNumber = documentNumber?.Trim() ?? string.Empty;
        if (!Application.Common.Validation.VerificationRules.SupportedPersonalDataTypes.Any(t =>
                string.Equals(t, documentType, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["ErrorMessage"] = "Неизвестный тип данных для верификации";
            return await VerifyDocuments(clientId);
        }

        // Получаем агента по email пользователя или создаем временного
        var agent = await _agentRepository.GetByEmailAsync(user.Email);
        if (agent == null)
        {
            var agents = await _agentRepository.GetAllAsync();
            agent = agents.FirstOrDefault();
            if (agent == null)
            {
                agent = new Agent(user.Username, user.Email);
                await _agentRepository.AddAsync(agent);
                await _agentRepository.SaveChangesAsync();
            }
        }

        try
        {
            var verification = new Domain.Entities.DocumentVerification(
                clientId,
                verifiedByAgentId: agent.Id, // Агент создает верификацию напрямую
                documentType,
                documentNumber,
                notes);

            await _verificationRepository.AddAsync(verification);
            await _verificationRepository.SaveChangesAsync();

            // Записываем в историю
            var history = new Domain.Entities.OperationHistory(
                userId,
                "VerifyDocument",
                $"Верификация документа: {documentType}",
                verification.Id,
                "DocumentVerification");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();

            return RedirectToAction("VerifyDocuments", new { clientId });
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return await VerifyDocuments(clientId);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveVerification(Guid id)
    {
        var verification = await _verificationRepository.GetByIdAsync(id);
        if (verification == null)
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        // Получаем агента по email пользователя или создаем временного
        var agent = await _agentRepository.GetByEmailAsync(user.Email);
        if (agent == null)
        {
            var agents = await _agentRepository.GetAllAsync();
            agent = agents.FirstOrDefault();
            if (agent == null)
            {
                agent = new Agent(user.Username, user.Email);
                await _agentRepository.AddAsync(agent);
                await _agentRepository.SaveChangesAsync();
            }
        }

        try
        {
            verification.Approve(agent.Id);
            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Верификация успешно одобрена";
            return RedirectToAction("VerifyDocuments", new { clientId = verification.ClientId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при одобрении верификации {VerificationId}", id);
            TempData["ErrorMessage"] = $"Ошибка при одобрении верификации: {ex.Message}";
            return RedirectToAction("VerifyDocuments", new { clientId = verification.ClientId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectVerification(Guid id, string reason)
    {
        var verification = await _verificationRepository.GetByIdAsync(id);
        if (verification == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Причина отклонения обязательна";
            return RedirectToAction("VerifyDocuments", new { clientId = verification.ClientId });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        // Получаем агента по email пользователя или создаем временного
        var agent = await _agentRepository.GetByEmailAsync(user.Email);
        if (agent == null)
        {
            var agents = await _agentRepository.GetAllAsync();
            agent = agents.FirstOrDefault();
            if (agent == null)
            {
                agent = new Agent(user.Username, user.Email);
                await _agentRepository.AddAsync(agent);
                await _agentRepository.SaveChangesAsync();
            }
        }

        try
        {
            verification.Reject(agent.Id, reason);
            await _verificationRepository.UpdateAsync(verification);
            await _verificationRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Верификация отклонена";
            return RedirectToAction("VerifyDocuments", new { clientId = verification.ClientId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отклонении верификации {VerificationId}", id);
            TempData["ErrorMessage"] = $"Ошибка при отклонении верификации: {ex.Message}";
            return RedirectToAction("VerifyDocuments", new { clientId = verification.ClientId });
        }
    }

    // Обработка заявок клиентов
    public async Task<IActionResult> Applications(string status = "")
    {
        var applications = await _applicationRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.ApplicationStatus>(status, out var statusEnum))
        {
            applications = applications.Where(a => a.Status == statusEnum).ToList();
        }

        ViewBag.Status = status;
        return View(applications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveApplication(Guid id)
    {
        var application = await _applicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        // Получаем агента по email пользователя или создаем временного
        var agent = await _agentRepository.GetByEmailAsync(user.Email);
        if (agent == null)
        {
            var agents = await _agentRepository.GetAllAsync();
            agent = agents.FirstOrDefault();
            if (agent == null)
            {
                agent = new Agent(user.Username, user.Email);
                await _agentRepository.AddAsync(agent);
                await _agentRepository.SaveChangesAsync();
            }
        }

        try
        {
            application.Approve(agent.Id);
            await _applicationRepository.UpdateAsync(application);
            await _applicationRepository.SaveChangesAsync();

            // Записываем в историю
            var history = new OperationHistory(
                userId,
                "ApproveApplication",
                $"Одобрена заявка на договор",
                application.Id,
                "ContractApplication");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка успешно одобрена";
            return RedirectToAction("Applications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при одобрении заявки {ApplicationId}", id);
            TempData["ErrorMessage"] = $"Ошибка при одобрении заявки: {ex.Message}";
            return RedirectToAction("Applications");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectApplication(Guid id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Причина отклонения обязательна";
            return RedirectToAction("Applications");
        }

        var application = await _applicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            application.Reject(reason);
            await _applicationRepository.UpdateAsync(application);
            await _applicationRepository.SaveChangesAsync();

            // Записываем в историю
            var history = new OperationHistory(
                userId,
                "RejectApplication",
                $"Отклонена заявка на договор: {reason}",
                application.Id,
                "ContractApplication");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка отклонена";
            return RedirectToAction("Applications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отклонении заявки {ApplicationId}", id);
            TempData["ErrorMessage"] = $"Ошибка при отклонении заявки: {ex.Message}";
            return RedirectToAction("Applications");
        }
    }

    // Активация договора
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateContract(Guid id)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        if (contract == null)
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _contractService.ActivateContractAsync(id, _verificationRepository);

            // Записываем в историю
            var history = new OperationHistory(
                userId,
                "ActivateContract",
                $"Активирован договор: {contract.Number ?? "без номера"}",
                contract.Id,
                "Contract");
            await _historyRepository.AddAsync(history);
            await _historyRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Договор успешно активирован";
            return RedirectToAction("Contracts");
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Ошибка домена при активации договора {ContractId}", id);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Contracts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при активации договора {ContractId}", id);
            TempData["ErrorMessage"] = $"Ошибка при активации договора: {ex.Message}";
            return RedirectToAction("Contracts");
        }
    }
}

