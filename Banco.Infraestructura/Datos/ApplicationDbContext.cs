using Banco.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Banco.Infraestructura.Datos;

public class ApplicationDbContext : DbContext
{
    // Pasamos las opciones de configuración (como la cadena de conexión) al constructor base
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    // Aquí registramos nuestras tablas. DbSet representa la colección de todas las facturas.
    public DbSet<Factura> Facturas { get; set; }
    
    // Este método permite configurar cómo se mapean las propiedades a las columnas
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Le decimos que el Id es la llave primaria
        modelBuilder.Entity<Factura>().HasKey(f => f.Id);
        
        // Le indicamos que el monto total requiere precisión decimal exacta
        modelBuilder.Entity<Factura>().Property(f => f.MontoTotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Factura>().Property(f => f.SaldoPendiente).HasColumnType("decimal(18,2)");
    }
}