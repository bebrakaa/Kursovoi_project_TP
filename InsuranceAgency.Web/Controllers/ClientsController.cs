using InsuranceAgency.Application.DTOs.Client;
using InsuranceAgency.Application.Interfaces.Repositories;
using AutoMapper;
using InsuranceAgency.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _clientRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(
        IClientRepository clientRepository,
        IMapper mapper,
        ILogger<ClientsController> logger)
    {
        _clientRepository = clientRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Получить всех клиентов
    /// Только для администратора.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var clients = await _clientRepository.GetAllAsync();
        var dto = _mapper.Map<IEnumerable<ClientDto>>(clients);
        return Ok(dto);
    }

    /// <summary>
    /// Получить клиента по ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound(new { message = $"Client with id {id} not found" });
        }

        var dto = _mapper.Map<ClientDto>(client);
        return Ok(dto);
    }

    /// <summary>
    /// Создать нового клиента
    /// Доступно админу и агенту (например, при регистрации в офисе).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var client = new Client(
                request.FullName,
                request.Email,
                request.Phone,
                request.Passport);

            await _clientRepository.AddAsync(client);
            await _clientRepository.SaveChangesAsync();

            var dto = _mapper.Map<ClientDto>(client);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновить данные клиента
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] CreateClientRequest request)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound(new { message = $"Client with id {id} not found" });
        }

        client.UpdateContact(request.FullName, request.Email, request.Phone);
        if (!string.IsNullOrWhiteSpace(request.Passport))
        {
            client.SetPassport(request.Passport);
        }

        await _clientRepository.UpdateAsync(client);
        await _clientRepository.SaveChangesAsync();

        var dto = _mapper.Map<ClientDto>(client);
        return Ok(dto);
    }

    /// <summary>
    /// Удалить клиента
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Agent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client == null)
        {
            return NotFound(new { message = $"Client with id {id} not found" });
        }

        await _clientRepository.DeleteAsync(client);
        await _clientRepository.SaveChangesAsync();

        return NoContent();
    }
}

// Request DTO for creating client
public class CreateClientRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Passport { get; set; }
}

