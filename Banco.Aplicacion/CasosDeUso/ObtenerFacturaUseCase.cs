using Banco.Aplicacion.Repositorios;
using Banco.Aplicacion.DTOs;

namespace Banco.Aplicacion.CasosDeUso;

public class ObtenerFacturaUseCase
{
    private readonly IFacturaRepository _facturaRepository;

    public ObtenerFacturaUseCase(IFacturaRepository facturaRepository)
    {
        _facturaRepository = facturaRepository;
    }

    public async Task<FacturaResponseDto> EjecutarAsync(Guid id)
    {
        var factura = await _facturaRepository.ObtenerPorIdAsync(id);

        if (factura == null) return null;

        return new FacturaResponseDto
        {
            Id = factura.Id,
            ClienteId = factura.ClienteId,
            MontoTotal = factura.MontoTotal,
            SaldoPendiente = factura.SaldoPendiente,
            Estado = factura.Estado.ToString()
        };
    }
}