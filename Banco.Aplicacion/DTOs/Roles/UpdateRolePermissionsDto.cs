namespace Banco.Aplicacion.DTOs.Roles;

public class UpdateRolePermissionsDto
{
    public List<Guid> PermissionIds { get; set; } = new();
}
