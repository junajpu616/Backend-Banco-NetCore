namespace Banco.API.Controllers;

using System.Security.Claims;
using Banco.Aplicacion.CasosDeUso.Accounts;
using Banco.Aplicacion.DTOs.Accounts;
using Banco.Dominio.Constantes;
using Banco.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly CreateAccountUseCase _createAccount;
    private readonly GetAccountsUseCase _getAccounts;

    public AccountsController(CreateAccountUseCase createAccount, GetAccountsUseCase getAccounts)
    {
        _createAccount = createAccount;
        _getAccounts = getAccounts;
    }

    /// <summary>
    /// [Admin, Supervisor] Lista todas las cuentas del sistema.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor}")]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _getAccounts.ExecuteAllAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// [Admin, Supervisor] Obtiene una cuenta por Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _getAccounts.ExecuteByIdAsync(id);
        return account is null ? NotFound() : Ok(account);
    }

    /// <summary>
    /// [Cliente] Lista únicamente las cuentas propias del cliente autenticado.
    /// </summary>
    [HttpGet("my-accounts")]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> GetMyAccounts()
    {
        var userId = GetCurrentUserId();
        var accounts = await _getAccounts.ExecuteByUserAsync(userId);
        return Ok(accounts);
    }

    /// <summary>
    /// [Admin] Crea una cuenta bancaria para un usuario existente.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
    {
        try
        {
            var executedBy = GetCurrentUserId();
            var result = await _createAccount.ExecuteAsync(dto, executedBy);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue("userId")!);
}
