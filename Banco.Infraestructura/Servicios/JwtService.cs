namespace Banco.Infraestructura.Servicios;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Banco.Aplicacion.Servicios;
using Banco.Dominio.Constantes;
using Banco.Dominio.Entidades;
using Banco.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public JwtService(IConfiguration config, ApplicationDbContext db)
    {
        _config = config;
        _db = db;
    }

    public string GenerateToken(User user)
    {
        var secret = _config["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret no configurado.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roleName = user.RoleDefinition?.Name ?? user.Role;
        var permissionClaims = GetPermissionClaimsAsync(user.RoleId, roleName).GetAwaiter().GetResult();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, roleName),
            // Also add the standard 'role' claim to satisfy different role mappings
            new Claim("role", roleName),
            new Claim("userId", user.Id.ToString()),
            new Claim("fullName", $"{user.FirstName} {user.LastName}")
        };

        claims.AddRange(permissionClaims.Select(permission => new Claim("permission", permission)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<List<string>> GetPermissionClaimsAsync(Guid? roleId, string roleName)
    {
        if (roleId is null)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(role => role.Name == roleName);
            roleId = role?.Id;
        }

        if (roleId is null)
        {
            return new List<string>();
        }

        return await _db.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();
    }
}
