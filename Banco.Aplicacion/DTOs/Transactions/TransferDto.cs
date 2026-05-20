namespace Banco.Aplicacion.DTOs.Transactions;

public class TransferDto
{
    public Guid SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
}
