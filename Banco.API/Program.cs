using Banco.Aplicacion.CasosDeUso;
using Banco.Aplicacion.Repositorios;
using Banco.Infraestructura.Datos;
using Banco.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Leer la cadena de conexión y configurar PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
    
// Inyección de dependencias
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<RegistrarPagoUseCase>();

// Configurar controladores y swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
