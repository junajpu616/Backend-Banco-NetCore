using Banco.Aplicacion.CasosDeUso;
using Microsoft.AspNetCore.Mvc;

namespace Banco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly RegistrarPagoUseCase _registrarPagoUseCase;

    // Inyectamos el caso de uso de la capa de aplicación
    public FacturasController(RegistrarPagoUseCase registrarPagoUseCase)
    {
        _registrarPagoUseCase = registrarPagoUseCase;
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