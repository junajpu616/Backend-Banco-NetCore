namespace Banco.Aplicacion.CasosDeUso.Users;

using Banco.Aplicacion.DTOs.Users;
using Banco.Aplicacion.Repositorios;
using Banco.Aplicacion.Servicios;
using Banco.Dominio.Constantes;
using Banco.Dominio.Entidades;
using Banco.Dominio.Excepciones;

public class CreateUserUseCase
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IPasswordHasher _passwordHasher;

    private static readonly HashSet<string> ValidRoles =
        new() { Roles.Admin, Roles.Supervisor, Roles.Cajero, Roles.Cliente };

    public CreateUserUseCase(IUserRepository userRepo, IRoleRepository roleRepo, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> ExecuteAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            throw new BusinessException("Nombre y apellido son requeridos.");

        if (!ValidRoles.Contains(dto.Role))
            throw new BusinessException($"Rol inválido. Válidos: {string.Join(", ", ValidRoles)}");

        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new BusinessException($"El email '{dto.Email}' ya está registrado.");

        var role = await _roleRepo.GetByNameAsync(dto.Role)
            ?? throw new BusinessException($"Rol inválido. Válidos: {string.Join(", ", ValidRoles)}");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            Role = dto.Role,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);

        return MapToDto(user);
    }

    internal static UserResponseDto MapToDto(User u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Role = u.RoleDefinition?.Name ?? u.Role,
        CreatedAt = u.CreatedAt
    };
}
