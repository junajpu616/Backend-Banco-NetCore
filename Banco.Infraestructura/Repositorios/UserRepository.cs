namespace Banco.Infraestructura.Repositorios;

using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _db.Users.Include(user => user.RoleDefinition).FirstOrDefaultAsync(user => user.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _db.Users.Include(user => user.RoleDefinition).FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant());

    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _db.Users.Include(user => user.RoleDefinition).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();

    public async Task<User> AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> EmailExistsAsync(string email) =>
        await _db.Users.AnyAsync(u => u.Email == email.Trim().ToLowerInvariant());
}
