namespace Banco.API.Controllers;

using System.Security.Claims;
using Banco.Aplicacion.CasosDeUso.Users;
using Banco.Aplicacion.DTOs.Users;
using Banco.Dominio.Constantes;
using Banco.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly CreateUserUseCase _createUser;
    private readonly GetUsersUseCase _getUsers;

    public UsersController(CreateUserUseCase createUser, GetUsersUseCase getUsers)
    {
        _createUser = createUser;
        _getUsers = getUsers;
    }

    /// <summary>
    /// [Admin, Supervisor] Lista todos los usuarios del sistema.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor}")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _getUsers.ExecuteAsync();
        return Ok(users);
    }

    /// <summary>
    /// [Admin, Supervisor] Obtiene un usuario por su Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Supervisor}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _getUsers.ExecuteByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Devuelve el perfil del usuario autenticado.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        var user = await _getUsers.ExecuteByIdAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// [Admin] Crea un usuario con cualquier rol.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var result = await _createUser.ExecuteAsync(dto);
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
