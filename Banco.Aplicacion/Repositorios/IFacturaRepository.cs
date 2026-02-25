using Banco.Dominio.Entidades;

namespace Banco.Aplicacion.Repositorios;

public interface IFacturaRepository
{
    Task<Factura> ObtenerPorIdAsync(Guid id);
    Task ActualizarAsync(Factura factura);
    Task AgregarAsync(Factura factura);
}