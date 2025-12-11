using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly IInsuranceServiceRepository _serviceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IAgentRepository _agentRepository;

    public AdminController(
        IInsuranceServiceRepository serviceRepository,
        IClientRepository clientRepository,
        IContractRepository contractRepository,
        IAgentRepository agentRepository)
    {
        _serviceRepository = serviceRepository;
        _clientRepository = clientRepository;
        _contractRepository = contractRepository;
        _agentRepository = agentRepository;
    }

    // Справочник услуг страхования
    public async Task<IActionResult> Services()
    {
        var services = await _serviceRepository.GetAllAsync();
        return View(services);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService(Application.DTOs.InsuranceService.CreateServiceDto dto)
    {
        if (!ModelState.IsValid)
        {
            var services = await _serviceRepository.GetAllAsync();
            return View("Services", services);
        }

        try
        {
            var service = new InsuranceService(dto.Name, new Money(dto.DefaultPremium, "RUB"), dto.Description);
            await _serviceRepository.AddAsync(service);
            await _serviceRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Услуга успешно добавлена";
            return RedirectToAction("Services");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при создании услуги: {ex.Message}");
            var services = await _serviceRepository.GetAllAsync();
            return View("Services", services);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateService(Guid id, Application.DTOs.InsuranceService.CreateServiceDto dto)
    {
        if (!ModelState.IsValid)
        {
            var services = await _serviceRepository.GetAllAsync();
            return View("Services", services);
        }

        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        try
        {
            service.UpdateName(dto.Name);
            service.UpdateDescription(dto.Description);
            service.UpdateDefaultPremium(new Money(dto.DefaultPremium, "RUB"));

            await _serviceRepository.UpdateAsync(service);
            await _serviceRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Услуга успешно обновлена";
            return RedirectToAction("Services");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при обновлении услуги: {ex.Message}");
            var services = await _serviceRepository.GetAllAsync();
            return View("Services", services);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        try
        {
            await _serviceRepository.DeleteAsync(service);
            await _serviceRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Услуга успешно удалена";
            return RedirectToAction("Services");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка при удалении услуги: {ex.Message}";
            return RedirectToAction("Services");
        }
    }
}

