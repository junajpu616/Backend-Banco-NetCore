namespace Banco.Aplicacion.DTOs.Permissions;

public class PermissionSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
