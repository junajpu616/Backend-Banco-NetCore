namespace Banco.Aplicacion.Servicios;

using Banco.Aplicacion.DTOs.Transactions;

public interface ITransactionService
{
    Task<TransactionResponseDto> DepositAsync(Guid accountId, decimal amount, Guid executedById);
    Task<TransactionResponseDto> WithdrawAsync(Guid accountId, decimal amount, Guid executedById);
    Task<TransactionResponseDto> TransferAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        Guid executedById,
        Guid? ownerCheckUserId = null);
    Task<IEnumerable<TransactionResponseDto>> GetAllAsync();
    Task<IEnumerable<TransactionResponseDto>> GetByAccountIdAsync(Guid accountId, Guid? ownerCheckUserId = null);
}
