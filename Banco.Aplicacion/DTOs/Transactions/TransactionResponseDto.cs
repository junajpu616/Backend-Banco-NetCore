namespace Banco.Aplicacion.DTOs.Transactions;

public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public Guid? SourceAccountId { get; set; }
    public string? SourceAccountNumber { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid ExecutedById { get; set; }
    public string ExecutedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
