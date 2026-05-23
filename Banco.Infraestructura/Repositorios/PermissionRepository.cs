namespace Banco.Infraestructura.Repositorios;

using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _db;

    public PermissionRepository(ApplicationDbContext db) => _db = db;

    public async Task<Permission?> GetByIdAsync(Guid id) =>
        await _db.Permissions.FindAsync(id);

    public async Task<Permission?> GetByCodeAsync(string code) =>
        await _db.Permissions.FirstOrDefaultAsync(permission => permission.Code == code.Trim());

    public async Task<IEnumerable<Permission>> GetAllAsync() =>
        await _db.Permissions.OrderBy(permission => permission.Code).ToListAsync();

    public async Task<Permission> AddAsync(Permission permission)
    {
        _db.Permissions.Add(permission);
        await _db.SaveChangesAsync();
        return permission;
    }

    public async Task UpdateAsync(Permission permission)
    {
        _db.Permissions.Update(permission);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Permission permission)
    {
        _db.Permissions.Remove(permission);
        await _db.SaveChangesAsync();
    }
}
