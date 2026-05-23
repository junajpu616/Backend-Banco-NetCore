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

    public async Task<Account> AddWithInitialDepositAsync(Account account, decimal initialDeposit, Guid executedById)
    {
        // Create account and optional initial deposit in a single DB transaction
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Accounts.Add(account);

            if (initialDeposit > 0)
            {
                account.Balance = initialDeposit;

                var deposit = new Transaction
                {
                    Id = Guid.NewGuid(),
                    DestinationAccountId = account.Id,
                    Amount = initialDeposit,
                    TransactionType = "Deposit",
                    ExecutedById = executedById,
                    Timestamp = DateTime.UtcNow
                };

                _db.Transactions.Add(deposit);
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return account;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> AccountNumberExistsAsync(string accountNumber) =>
        await _db.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);

    public async Task<Account?> GetByNumberAsync(string accountNumber)
    {
        return await _db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }
}
