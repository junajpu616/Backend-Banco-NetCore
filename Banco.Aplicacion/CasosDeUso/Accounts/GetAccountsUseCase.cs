namespace Banco.Aplicacion.CasosDeUso.Accounts;

using Banco.Aplicacion.DTOs.Accounts;
using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;

public class GetAccountsUseCase
{
    private readonly IAccountRepository _accountRepo;

    public GetAccountsUseCase(IAccountRepository accountRepo)
    {
        _accountRepo = accountRepo;
    }

    public async Task<IEnumerable<AccountResponseDto>> ExecuteAllAsync()
    {
        var accounts = await _accountRepo.GetAllAsync();
        return accounts.Select(Map);
    }

    public async Task<IEnumerable<AccountResponseDto>> ExecuteByUserAsync(Guid userId)
    {
        var accounts = await _accountRepo.GetByUserIdAsync(userId);
        return accounts.Select(Map);
    }

    public async Task<AccountResponseDto?> ExecuteByIdAsync(Guid id)
    {
        var account = await _accountRepo.GetByIdWithUserAsync(id);
        return account is null ? null : Map(account);
    }

    public async Task<AccountResponseDto?> ExecuteByNumberAsync(string number)
    {
        var account = await _accountRepo.GetByNumberAsync(number);
        return account is null ? null : Map(account);
    }

    private static AccountResponseDto Map(Account a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        OwnerFullName = a.User is not null ? $"{a.User.FirstName} {a.User.LastName}" : string.Empty,
        AccountNumber = a.AccountNumber,
        AccountType = a.AccountType,
        Balance = a.Balance
    };
}
