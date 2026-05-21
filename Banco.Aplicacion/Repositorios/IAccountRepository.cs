namespace Banco.Aplicacion.Repositorios;

using Banco.Dominio.Entidades;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByIdWithUserAsync(Guid id);
    Task<IEnumerable<Account>> GetAllAsync();
    Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId);
    Task<Account> AddAsync(Account account);
    Task<Account> AddWithInitialDepositAsync(Account account, decimal initialDeposit, Guid executedById);
    Task<bool> AccountNumberExistsAsync(string accountNumber);
}
