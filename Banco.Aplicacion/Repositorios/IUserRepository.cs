namespace Banco.Aplicacion.Repositorios;

using Banco.Dominio.Entidades;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> AddAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}
