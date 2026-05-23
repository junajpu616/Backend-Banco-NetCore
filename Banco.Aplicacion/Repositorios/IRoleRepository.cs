namespace Banco.Aplicacion.Repositorios;

using Banco.Dominio.Entidades;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByIdWithPermissionsAsync(Guid id);
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Role role);
    Task SetPermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds);
}
