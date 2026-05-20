namespace Banco.Aplicacion.CasosDeUso.Auth;

using Banco.Aplicacion.DTOs.Auth;
using Banco.Aplicacion.Repositorios;
using Banco.Aplicacion.Servicios;
using Banco.Dominio.Excepciones;

public class LoginUseCase
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginUseCase(IUserRepository userRepo, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<TokenResponseDto> ExecuteAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new BusinessException("Credenciales inválidas.");

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new BusinessException("Credenciales inválidas.");

        var token = _jwtService.GenerateToken(user);

        return new TokenResponseDto
        {
            Token = token,
            Role = user.Role,
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };
    }
}
