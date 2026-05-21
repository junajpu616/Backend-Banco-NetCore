namespace Banco.Aplicacion.CasosDeUso.Accounts;

using Banco.Aplicacion.DTOs.Accounts;
using Banco.Aplicacion.Repositorios;
using System.Linq;

public class GetAccountRequestsUseCase
{
    private readonly IAccountRequestRepository _requestRepo;

    public GetAccountRequestsUseCase(IAccountRequestRepository requestRepo)
    {
        _requestRepo = requestRepo;
    }

    public async Task<IEnumerable<AccountRequestResponseDto>> ExecuteAsync()
    {
        var list = await _requestRepo.GetAllAsync();
        // Return only pending requests by default so processed ones don't clutter the admin view
        var pending = list.Where(r => r.Status == "Pending");
        return pending.Select(r => new AccountRequestResponseDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserFullName = r.User is null ? string.Empty : $"{r.User.FirstName} {r.User.LastName}",
            AccountType = r.AccountType,
            InitialBalance = r.InitialBalance,
            Status = r.Status,
            ReviewedAt = r.ReviewedAt,
            ReviewedById = r.ReviewedById,
            Note = r.Note
        });
    }
}
