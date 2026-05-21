namespace Banco.Infraestructura.Repositorios;

using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

public class AccountRequestRepository : IAccountRequestRepository
{
    private readonly ApplicationDbContext _db;

    public AccountRequestRepository(ApplicationDbContext db) => _db = db;

    public async Task<AccountRequest> AddAsync(AccountRequest request)
    {
        _db.AccountRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<AccountRequest?> GetByIdAsync(Guid id) =>
        await _db.AccountRequests.FindAsync(id);

    public async Task<IEnumerable<AccountRequest>> GetAllAsync() =>
        await _db.AccountRequests.Include(r => r.User).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task UpdateAsync(AccountRequest request)
    {
        _db.AccountRequests.Update(request);
        await _db.SaveChangesAsync();
    }
}
