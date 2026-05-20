namespace Banco.Infraestructura.Repositorios;

using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _db;

    public AccountRepository(ApplicationDbContext db) => _db = db;

    public async Task<Account?> GetByIdAsync(Guid id) =>
        await _db.Accounts.FindAsync(id);

    public async Task<Account?> GetByIdWithUserAsync(Guid id) =>
        await _db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Account>> GetAllAsync() =>
        await _db.Accounts.Include(a => a.User).OrderBy(a => a.AccountNumber).ToListAsync();

    public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId) =>
        await _db.Accounts.Include(a => a.User).Where(a => a.UserId == userId).ToListAsync();

    public async Task<Account> AddAsync(Account account)
    {
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task<bool> AccountNumberExistsAsync(string accountNumber) =>
        await _db.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);
}
