using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServicesController : ControllerBase
{
    private readonly IInsuranceServiceRepository _serviceRepository;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(
        IInsuranceServiceRepository serviceRepository,
        ILogger<ServicesController> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Получить все страховые услуги
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InsuranceServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InsuranceServiceDto>>> GetAll()
    {
        var services = await _serviceRepository.GetAllAsync();
        var dto = services.Select(s => new InsuranceServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            DefaultPremiumAmount = s.DefaultPremium.Amount,
            DefaultPremiumCurrency = s.DefaultPremium.Currency
        }).ToList();

        return Ok(dto);
    }

    /// <summary>
    /// Получить страховую услугу по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InsuranceServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InsuranceServiceDto>> GetById(Guid id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound(new { message = $"Insurance service with id {id} not found" });
        }

        var dto = new InsuranceServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            DefaultPremiumAmount = service.DefaultPremium.Amount,
            DefaultPremiumCurrency = service.DefaultPremium.Currency
        };

        return Ok(dto);
    }

    /// <summary>
    /// Добавить новую страховую услугу. Только для администратора.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(typeof(InsuranceServiceDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InsuranceServiceDto>> Create([FromBody] CreateServiceRequest request)
    {
        var service = new InsuranceService(
            request.Name,
            new Money(request.DefaultPremiumAmount, request.DefaultPremiumCurrency),
            request.Description);

        await _serviceRepository.AddAsync(service);
        await _serviceRepository.SaveChangesAsync();

        var dto = new InsuranceServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            DefaultPremiumAmount = service.DefaultPremium.Amount,
            DefaultPremiumCurrency = service.DefaultPremium.Currency
        };

        return CreatedAtAction(nameof(GetById), new { id = service.Id }, dto);
    }
}

// DTO for Insurance Service
public class InsuranceServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal DefaultPremiumAmount { get; set; }
    public string DefaultPremiumCurrency { get; set; } = "RUB";
}

public class CreateServiceRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal DefaultPremiumAmount { get; set; }
    public string DefaultPremiumCurrency { get; set; } = "RUB";
}

