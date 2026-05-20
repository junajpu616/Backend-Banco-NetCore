namespace Banco.Aplicacion.Servicios;

using Banco.Dominio.Entidades;

public interface IJwtService
{
    string GenerateToken(User user);
}
