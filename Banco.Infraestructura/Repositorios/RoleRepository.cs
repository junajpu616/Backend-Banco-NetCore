namespace Banco.Infraestructura.Repositorios;

using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _db;

    public RoleRepository(ApplicationDbContext db) => _db = db;

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id) =>
        await _db.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(role => role.Id == id);

    public async Task<Role?> GetByIdAsync(Guid id) =>
        await GetByIdWithPermissionsAsync(id);

    public async Task<Role?> GetByNameAsync(string name) =>
        await _db.Roles.FirstOrDefaultAsync(role => role.Name == name.Trim());

    public async Task<IEnumerable<Role>> GetAllAsync() =>
        await _db.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderBy(role => role.Name)
            .ToListAsync();

    public async Task<Role> AddAsync(Role role)
    {
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task UpdateAsync(Role role)
    {
        _db.Roles.Update(role);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Role role)
    {
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
    }

    public async Task SetPermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
    {
        var role = await _db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId)
            ?? throw new InvalidOperationException("Rol no encontrado.");

        var existing = role.RolePermissions.ToList();
        _db.RolePermissions.RemoveRange(existing);

        var distinctPermissionIds = permissionIds.Distinct().ToList();
        var newAssignments = distinctPermissionIds.Select(permissionId => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId
        });

        await _db.RolePermissions.AddRangeAsync(newAssignments);
        await _db.SaveChangesAsync();

        // Legacy `Role` column removed in schema; nothing to backfill here.
    }
}
