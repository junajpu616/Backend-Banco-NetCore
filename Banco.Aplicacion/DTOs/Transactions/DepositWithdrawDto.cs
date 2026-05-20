namespace Banco.Aplicacion.DTOs.Transactions;

public class DepositWithdrawDto
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}
