namespace Banco.Dominio.Entidades;

public class AccountRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
