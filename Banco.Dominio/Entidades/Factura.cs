namespace Banco.Dominio.Entidades;

public class Factura
{
    // Las propiedades tienen 'private set' para que nadie desde afuera
    // pueda alterarlas directamente sin pasar por las reglas de negocio.
    public Guid Id { get; private set; }
    public string ClienteId { get; private set;  }
    public decimal MontoTotal { get; private set; }
    public decimal SaldoPendiente { get; private set; }
    public DateTime FechaEmision { get; private set; }
    public DateTime FechaVencimiento { get; private set; }
    public EstadoFactura Estado { get; private set; }

    // Constructor: Obliga a que cuando se cree una factura, nazca con datos válidos.
    public Factura(string clienteId, decimal montoTotal, DateTime fechaVencimiento)
    {
        if (montoTotal <= 0)
            throw new ArgumentException("El monto de la factura debe sermayor a cero.");

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        MontoTotal = montoTotal;
        SaldoPendiente = montoTotal; // Al inicio, se debe todo el monto
        FechaEmision = DateTime.UtcNow;
        FechaVencimiento = fechaVencimiento;
        Estado = EstadoFactura.Pendiente;
    }
    
    // Comportamiento (Regla de Negocio): Así es como interactuamos con la entidad
    public void RegistrarPago(decimal montoPago)
    {
        if (montoPago <= 0)
            throw new ArgumentException("El monto del pago debe ser mayor a cero.");
        if (montoPago > SaldoPendiente)
            throw new InvalidOperationException("El monto del pago no puede ser mayor al saldo pendiente.");
        
        SaldoPendiente -= montoPago;

        if (SaldoPendiente == 0)
        {
            Estado = EstadoFactura.Pagada;
        }
    }
}

// Enum sencillo para manejar los estados
public enum EstadoFactura
{
    Pendiente,
    Pagada,
    EnMora,
    Anulada
}