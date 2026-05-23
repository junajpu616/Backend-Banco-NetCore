namespace Banco.Aplicacion.DTOs.Roles;

using Banco.Aplicacion.DTOs.Permissions;

public class RoleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<PermissionSummaryDto> Permissions { get; set; } = new();
}
