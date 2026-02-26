using Banco.Dominio.Entidades;
using Banco.Aplicacion.Repositorios;

namespace Banco.Aplicacion.CasosDeUso;

public class CrearFacturaUseCase
{
    private readonly IFacturaRepository _facturaRepository;

    public CrearFacturaUseCase(IFacturaRepository facturaRepository)
    {
        _facturaRepository = facturaRepository;
    }

    public async Task<Guid> EjecutarAsync(string clienteId, decimal montoTotal, DateTime fechaVencimiento)
    {
        var nuevaFactura = new Factura(clienteId, montoTotal, fechaVencimiento);

        await _facturaRepository.AgregarAsync(nuevaFactura);

        return nuevaFactura.Id;
    }
}