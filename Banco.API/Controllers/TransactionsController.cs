namespace Banco.API.Controllers;

using System.Security.Claims;
using Banco.Aplicacion.DTOs.Transactions;
using Banco.Aplicacion.Servicios;
using Banco.Dominio.Constantes;
using Banco.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _txService;

    public TransactionsController(ITransactionService txService) => _txService = txService;

    /// <summary>
    /// [Admin, Supervisor] Historial completo de todas las transacciones.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor}")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _txService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// [Admin, Supervisor] Historial de transacciones de cualquier cuenta.
    /// [Cliente] Solo puede consultar sus propias cuentas (validado en el servicio).
    /// </summary>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetByAccount(Guid accountId)
    {
        try
        {
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            Guid? ownerCheck = role == Roles.Cliente ? GetCurrentUserId() : null;

            var result = await _txService.GetByAccountIdAsync(accountId, ownerCheck);
            return Ok(result);
        }
        catch (BusinessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// [Admin, Supervisor, Cajero] Realiza un depósito en efectivo en la cuenta indicada.
    /// </summary>
    [HttpPost("deposit")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor},{Roles.Cajero}")]
    public async Task<IActionResult> Deposit([FromBody] DepositWithdrawDto dto)
    {
        try
        {
            var result = await _txService.DepositAsync(dto.AccountId, dto.Amount, GetCurrentUserId());
            return Ok(result);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin, Supervisor, Cajero] Realiza un retiro en efectivo de la cuenta indicada.
    /// </summary>
    [HttpPost("withdraw")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor},{Roles.Cajero}")]
    public async Task<IActionResult> Withdraw([FromBody] DepositWithdrawDto dto)
    {
        try
        {
            var result = await _txService.WithdrawAsync(dto.AccountId, dto.Amount, GetCurrentUserId());
            return Ok(result);
        }
        catch (InsufficientFundsException ex)
        {
            return BadRequest(new { message = ex.Message, available = ex.Available, requested = ex.Requested });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Cliente] Transfiere fondos desde una cuenta propia hacia cualquier cuenta destino.
    /// El servicio valida que la cuenta origen pertenezca al cliente autenticado.
    /// </summary>
    [HttpPost("transfer")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> Transfer([FromBody] TransferDto dto)
    {
        try
        {
            var callerId = GetCurrentUserId();
            var result = await _txService.TransferAsync(
                dto.SourceAccountId,
                dto.DestinationAccountId,
                dto.Amount,
                callerId,
                ownerCheckUserId: callerId);

            return Ok(result);
        }
        catch (InsufficientFundsException ex)
        {
            return BadRequest(new { message = ex.Message, available = ex.Available, requested = ex.Requested });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue("userId")!);
}
