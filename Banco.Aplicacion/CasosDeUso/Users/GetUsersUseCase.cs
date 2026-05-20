namespace Banco.Aplicacion.CasosDeUso.Users;

using Banco.Aplicacion.DTOs.Users;
using Banco.Aplicacion.Repositorios;

public class GetUsersUseCase
{
    private readonly IUserRepository _userRepo;

    public GetUsersUseCase(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IEnumerable<UserResponseDto>> ExecuteAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(CreateUserUseCase.MapToDto);
    }

    public async Task<UserResponseDto?> ExecuteByIdAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user is null ? null : CreateUserUseCase.MapToDto(user);
    }
}
