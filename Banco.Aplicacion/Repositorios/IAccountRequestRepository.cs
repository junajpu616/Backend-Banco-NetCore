namespace Banco.Aplicacion.Repositorios;

using Banco.Dominio.Entidades;

public interface IAccountRequestRepository
{
    Task<AccountRequest> AddAsync(AccountRequest request);
    Task<AccountRequest?> GetByIdAsync(Guid id);
    Task<IEnumerable<AccountRequest>> GetAllAsync();
    Task UpdateAsync(AccountRequest request);
}
