namespace Banco.Aplicacion.DTOs.Accounts;

public class CreateAccountRequestDto
{
    public Guid UserId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}
