namespace Banco.API.Controllers;

using Banco.Aplicacion.CasosDeUso.Auth;
using Banco.Aplicacion.DTOs.Auth;
using Banco.Dominio.Excepciones;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;

    public AuthController(LoginUseCase loginUseCase) => _loginUseCase = loginUseCase;

    /// <summary>Autentica un usuario y devuelve un token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _loginUseCase.ExecuteAsync(dto);
            return Ok(result);
        }
        catch (BusinessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
