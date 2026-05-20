namespace Banco.Dominio.Entidades;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid? SourceAccountId { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid ExecutedById { get; set; }
    public DateTime Timestamp { get; set; }

    public Account? SourceAccount { get; set; }
    public Account? DestinationAccount { get; set; }
    public User ExecutedBy { get; set; } = null!;
}
