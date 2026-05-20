namespace Banco.Aplicacion.CasosDeUso.Accounts;

using Banco.Aplicacion.DTOs.Accounts;
using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;
using Banco.Dominio.Excepciones;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _accountRepo;
    private readonly IUserRepository _userRepo;

    private static readonly HashSet<string> ValidTypes = new() { "Ahorro", "Corriente" };

    public CreateAccountUseCase(IAccountRepository accountRepo, IUserRepository userRepo)
    {
        _accountRepo = accountRepo;
        _userRepo = userRepo;
    }

    public async Task<AccountResponseDto> ExecuteAsync(CreateAccountDto dto)
    {
        if (!ValidTypes.Contains(dto.AccountType))
            throw new BusinessException("Tipo de cuenta inválido. Tipos válidos: Ahorro, Corriente");

        if (dto.InitialBalance < 0)
            throw new BusinessException("El saldo inicial no puede ser negativo.");

        var owner = await _userRepo.GetByIdAsync(dto.UserId)
            ?? throw new BusinessException("Usuario propietario no encontrado.");

        var accountNumber = await GenerateUniqueAccountNumberAsync();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            AccountNumber = accountNumber,
            AccountType = dto.AccountType,
            Balance = dto.InitialBalance
        };

        await _accountRepo.AddAsync(account);

        return new AccountResponseDto
        {
            Id = account.Id,
            UserId = account.UserId,
            OwnerFullName = $"{owner.FirstName} {owner.LastName}",
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            Balance = account.Balance
        };
    }

    private async Task<string> GenerateUniqueAccountNumberAsync()
    {
        var rng = Random.Shared;
        string number;
        do
        {
            // 10 dígitos: entre 1000000000 y 9999999999
            number = ((long)(rng.NextInt64(1_000_000_000L, 10_000_000_000L))).ToString();
        } while (await _accountRepo.AccountNumberExistsAsync(number));
        return number;
    }
}
