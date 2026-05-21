namespace Banco.Aplicacion.DTOs.Accounts;

public class AccountRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Note { get; set; }
}
