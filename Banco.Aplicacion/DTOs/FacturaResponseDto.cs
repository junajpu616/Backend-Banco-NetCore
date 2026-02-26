namespace Banco.Aplicacion.DTOs;

public class FacturaResponseDto
{
    public Guid Id { get; set; }
    public string ClienteId { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; }
}