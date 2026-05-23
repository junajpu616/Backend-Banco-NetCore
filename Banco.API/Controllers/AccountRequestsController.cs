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
public class AccountRequestsController : ControllerBase
{
    private readonly CreateAccountRequestUseCase _createRequest;
    private readonly GetAccountRequestsUseCase _getRequests;
    private readonly ApproveAccountRequestUseCase _approveRequest;

    public AccountRequestsController(
        CreateAccountRequestUseCase createRequest,
        GetAccountRequestsUseCase getRequests,
        ApproveAccountRequestUseCase approveRequest)
    {
        _createRequest = createRequest;
        _getRequests = getRequests;
        _approveRequest = approveRequest;
    }

    /// <summary>
    /// [Cliente] Crear una solicitud de cuenta.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Cliente)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequestDto dto)
    {
        try
        {
            var result = await _createRequest.ExecuteAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin] Lista todas las solicitudes.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Permissions.RequestsRead)]
    public async Task<IActionResult> GetAll()
    {
        var list = await _getRequests.ExecuteAsync();
        return Ok(list);
    }

    /// <summary>
    /// [Admin] Obtener solicitud por id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.RequestsRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var request = await _getRequests.ExecuteAsync();
        var item = request.FirstOrDefault(r => r.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>
    /// [Admin] Aprobar una solicitud.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = Permissions.AccountsApproveRequests)]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            var adminId = Guid.Parse(User.FindFirstValue("userId")!);
            var result = await _approveRequest.ApproveAsync(id, adminId);
            return Ok(result);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [Admin] Rechazar una solicitud.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = Permissions.AccountsApproveRequests)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? reason)
    {
        try
        {
            var adminId = Guid.Parse(User.FindFirstValue("userId")!);
            var result = await _approveRequest.RejectAsync(id, adminId, reason);
            return Ok(result);
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
