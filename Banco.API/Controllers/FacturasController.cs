using Banco.Aplicacion.CasosDeUso;
using Microsoft.AspNetCore.Mvc;

namespace Banco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly RegistrarPagoUseCase _registrarPagoUseCase;
    private readonly CrearFacturaUseCase _crearFacturaUseCase;
    private readonly ObtenerFacturaUseCase _obtenerFacturaUseCase;

    // Inyectamos el caso de uso de la capa de aplicación
    public FacturasController(
        RegistrarPagoUseCase registrarPagoUseCase,
        CrearFacturaUseCase crearFacturaUseCase,
        ObtenerFacturaUseCase obtenerFacturaUseCase)
    {
        _registrarPagoUseCase = registrarPagoUseCase;
        _crearFacturaUseCase = crearFacturaUseCase;
        _obtenerFacturaUseCase = obtenerFacturaUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(Guid id)
    {
        var factura = await _obtenerFacturaUseCase.EjecutarAsync(id);

        if (factura == null)
        {
            return NotFound(new { Mensaje = "La factura solicitada no existe." });
        }

        return Ok(factura);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PeticionCrearFactura request)
    {
        try
        {
            var nuevoId = await _crearFacturaUseCase.EjecutarAsync(
                request.ClienteId,
                request.MontoTotal,
                request.FechaVencimiento);
            return Ok(new { Mensaje = "Factura creada con éxito.", Id = nuevoId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/pagar")]
    public async Task<IActionResult> Pagar(Guid id, [FromBody] PeticionPago request)
    {
        try
        {
            await _registrarPagoUseCase.EjecutarAsync(id, request.Monto);

            return Ok(new { Mensaje = "Pago registrado exitosamente y saldo actualizado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}

// Un pequeño DTO para recibir el JSON del frontend
public class PeticionPago
{
    public decimal Monto { get; set; }
}

public class PeticionCrearFactura
{
    public string ClienteId { get; set; }
    public decimal MontoTotal { get; set; }
    public DateTime FechaVencimiento { get; set; }
}