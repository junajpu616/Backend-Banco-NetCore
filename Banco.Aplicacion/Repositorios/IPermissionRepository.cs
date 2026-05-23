namespace Banco.Aplicacion.Repositorios;

using Banco.Dominio.Entidades;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id);
    Task<Permission?> GetByCodeAsync(string code);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<Permission> AddAsync(Permission permission);
    Task UpdateAsync(Permission permission);
    Task DeleteAsync(Permission permission);
}
