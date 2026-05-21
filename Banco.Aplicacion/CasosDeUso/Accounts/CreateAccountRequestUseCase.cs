namespace Banco.Aplicacion.CasosDeUso.Accounts;

using Banco.Aplicacion.DTOs.Accounts;
using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Entidades;
using Banco.Dominio.Excepciones;

public class CreateAccountRequestUseCase
{
    private readonly IAccountRequestRepository _requestRepo;
    private static readonly HashSet<string> ValidTypes = new() { "Ahorro", "Corriente" };

    public CreateAccountRequestUseCase(IAccountRequestRepository requestRepo)
    {
        _requestRepo = requestRepo;
    }

    public async Task<AccountRequestResponseDto> ExecuteAsync(CreateAccountRequestDto dto)
    {
        if (!ValidTypes.Contains(dto.AccountType))
            throw new BusinessException("Tipo de cuenta inválido. Tipos válidos: Ahorro, Corriente");

        if (dto.InitialBalance < 0)
            throw new BusinessException("El saldo inicial no puede ser negativo.");

        var request = new AccountRequest
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            AccountType = dto.AccountType,
            InitialBalance = dto.InitialBalance,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _requestRepo.AddAsync(request);

        return new AccountRequestResponseDto
        {
            Id = request.Id,
            UserId = request.UserId,
            UserFullName = string.Empty,
            AccountType = request.AccountType,
            InitialBalance = request.InitialBalance,
            Status = request.Status,
            ReviewedAt = null
        };
    }
}
