namespace Banco.Dominio.Excepciones;

public class InsufficientFundsException : Exception
{
    public decimal Available { get; }
    public decimal Requested { get; }

    public InsufficientFundsException(decimal available, decimal requested)
        : base($"Saldo insuficiente. Disponible: {available:C2}, Solicitado: {requested:C2}")
    {
        Available = available;
        Requested = requested;
    }
}
