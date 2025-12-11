using InsuranceAgency.Application.DTOs;
using InsuranceAgency.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IContractService contractService,
        ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все договоры
    /// Доступно для аутентифицированных пользователей.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContractDto>>> GetAll()
    {
        var contracts = await _contractService.GetAllAsync();
        return Ok(contracts);
    }

    /// <summary>
    /// Получить договор по ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDto>> GetById(Guid id)
    {
        var contract = await _contractService.GetByIdAsync(id);
        if (contract == null)
        {
            return NotFound(new { message = $"Contract with id {id} not found" });
        }

        return Ok(contract);
    }

    /// <summary>
    /// Создать новый договор
    /// Доступно только агенту или администратору.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Agent,Admin")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContractDto>> Create([FromBody] CreateContractDto dto, [FromQuery] Guid agentId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var contract = await _contractService.CreateContractAsync(dto, agentId);
            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Зарегистрировать договор
    /// Доступно только агенту или администратору.
    /// </summary>
    [HttpPost("{id}/register")]
    [Authorize(Roles = "Agent,Admin")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractDto>> Register(Guid id, [FromBody] RegisterContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var contract = await _contractService.RegisterContractAsync(id, dto.Number, dto.AgentId);
            return Ok(contract);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering contract {ContractId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}

