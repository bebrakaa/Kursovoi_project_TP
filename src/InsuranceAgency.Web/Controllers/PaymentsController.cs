using InsuranceAgency.Application.DTOs.Payment;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAgency.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IPaymentRepository paymentRepository,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Инициировать платеж
    /// Доступно клиенту (Client) или администратору.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Client,Admin")]
    [ProducesResponseType(typeof(PaymentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentResultDto>> InitiatePayment([FromBody] InitiatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _paymentService.InitiatePaymentAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(new PaymentResultDto
                {
                    Success = false,
                    Error = result.Error
                });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for contract {ContractId}", dto.ContractId);
            return BadRequest(new PaymentResultDto
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Получить историю платежей по договору
    /// </summary>
    [HttpGet("by-contract/{contractId}")]
    [Authorize]
    public async Task<IActionResult> GetByContract(Guid contractId)
    {
        var payments = await _paymentRepository.GetByContractIdAsync(contractId);
        var dto = payments.Select(p => new
        {
            p.Id,
            p.Amount,
            p.Currency,
            Status = p.Status.ToString(),
            p.CreatedAt,
            p.UpdatedAt,
            p.PspTransactionId
        });

        return Ok(dto);
    }
}

