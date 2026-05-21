namespace Banco.Aplicacion.CasosDeUso.Accounts;

using Banco.Aplicacion.DTOs.Accounts;
using Banco.Aplicacion.Repositorios;
using Banco.Aplicacion.CasosDeUso.Accounts;
using Banco.Aplicacion.DTOs.Accounts;
using Banco.Dominio.Entidades;
using Banco.Dominio.Excepciones;

public class ApproveAccountRequestUseCase
{
    private readonly IAccountRequestRepository _requestRepo;
    private readonly CreateAccountUseCase _createAccount;

    public ApproveAccountRequestUseCase(IAccountRequestRepository requestRepo, CreateAccountUseCase createAccount)
    {
        _requestRepo = requestRepo;
        _createAccount = createAccount;
    }

    public async Task<AccountRequestResponseDto> ApproveAsync(Guid requestId, Guid adminUserId)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new BusinessException("Solicitud no encontrada.");

        if (request.Status != "Pending")
            throw new BusinessException("La solicitud ya fue procesada.");

        // Create the actual account using existing use case
        var createDto = new CreateAccountDto
        {
            UserId = request.UserId,
            AccountType = request.AccountType,
            InitialBalance = request.InitialBalance
        };

        var account = await _createAccount.ExecuteAsync(createDto, adminUserId);

        request.Status = "Approved";
        request.ReviewedById = adminUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.Note = $"Aprobada. Cuenta creada: {account.AccountNumber}";

        await _requestRepo.UpdateAsync(request);

        return new AccountRequestResponseDto
        {
            Id = request.Id,
            UserId = request.UserId,
            UserFullName = string.Empty,
            AccountType = request.AccountType,
            InitialBalance = request.InitialBalance,
            Status = request.Status,
            ReviewedAt = request.ReviewedAt,
            ReviewedById = request.ReviewedById,
            Note = request.Note
        };
    }

    public async Task<AccountRequestResponseDto> RejectAsync(Guid requestId, Guid adminUserId, string? reason = null)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new BusinessException("Solicitud no encontrada.");

        if (request.Status != "Pending")
            throw new BusinessException("La solicitud ya fue procesada.");

        request.Status = "Rejected";
        request.ReviewedById = adminUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.Note = reason;

        await _requestRepo.UpdateAsync(request);

        return new AccountRequestResponseDto
        {
            Id = request.Id,
            UserId = request.UserId,
            UserFullName = string.Empty,
            AccountType = request.AccountType,
            InitialBalance = request.InitialBalance,
            Status = request.Status,
            ReviewedAt = request.ReviewedAt,
            ReviewedById = request.ReviewedById,
            Note = request.Note
        };
    }
}
