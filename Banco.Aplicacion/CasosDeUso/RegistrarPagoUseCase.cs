using Banco.Dominio.Entidades;
using Banco.Aplicacion.Repositorios;

namespace Banco.Aplicacion.CasosDeUso;

public class RegistrarPagoUseCase
{
    private readonly IFacturaRepository _facturaRepository;

    public RegistrarPagoUseCase(IFacturaRepository facturaRepository)
    {
        _facturaRepository = facturaRepository;
    }

    public async Task EjecutarAsync(Guid facturaId, decimal montoPago)
    {
        var factura = await _facturaRepository.ObtenerPorIdAsync(facturaId);

        if (factura == null)
        {
            throw new Exception("La factura no fue encontrada");
        }
        
        factura.RegistrarPago(montoPago);

        await _facturaRepository.ActualizarAsync(factura);
    }
}