using Banco.Dominio.Entidades;
using Banco.Aplicacion.Repositorios;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace Banco.Infraestructura.Repositorios;

// Esta clase implementa la interfaz IFacturaRepository
public class FacturaRepository : IFacturaRepository
{
    private readonly ApplicationDbContext _context;

    // Inyectamos el DbContext que acabamos de crear
    public FacturaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Factura> ObtenerPorIdAsync(Guid id)
    {
        // FirstOrDefaultAsync busca el primer registro que coincida o devuelve null
        return await _context.Facturas.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task AgregarAsync(Factura factura)
    {
        await _context.Facturas.AddAsync(factura);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Factura factura)
    {
        _context.Facturas.Update(factura);
        await _context.SaveChangesAsync();
    }
}