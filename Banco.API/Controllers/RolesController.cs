namespace Banco.API.Controllers;

using Banco.Aplicacion.DTOs.Permissions;
using Banco.Aplicacion.DTOs.Roles;
using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Constantes;
using Banco.Dominio.Entidades;
using Banco.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.RbacRead)]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepo;
    private readonly IPermissionRepository _permissionRepo;

    public RolesController(IRoleRepository roleRepo, IPermissionRepository permissionRepo)
    {
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleRepo.GetAllAsync();
        return Ok(roles.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _roleRepo.GetByIdWithPermissionsAsync(id);
        return role is null ? NotFound() : Ok(MapToDto(role));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.RbacManage)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "El nombre del rol es requerido." });
            }

            if (await _roleRepo.GetByNameAsync(dto.Name) is not null)
            {
                return BadRequest(new { message = $"El rol '{dto.Name}' ya existe." });
            }

            var role = await _roleRepo.AddAsync(new Role
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                IsActive = true
            });

            var created = await _roleRepo.GetByIdWithPermissionsAsync(role.Id);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, MapToDto(created ?? role));
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.RbacManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _roleRepo.GetByIdWithPermissionsAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        role.Name = dto.Name.Trim();
        role.Description = dto.Description.Trim();
        role.IsActive = dto.IsActive;

        try
        {
            await _roleRepo.UpdateAsync(role);
            return Ok(MapToDto(role));
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "No se pudo actualizar el rol." });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.RbacManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        try
        {
            await _roleRepo.DeleteAsync(role);
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "No se puede eliminar un rol en uso." });
        }
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = Permissions.RbacManage)]
    public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] UpdateRolePermissionsDto dto)
    {
        var role = await _roleRepo.GetByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var permissions = await _permissionRepo.GetAllAsync();
        var validPermissionIds = permissions.Select(permission => permission.Id).ToHashSet();
        var requested = dto.PermissionIds.Distinct().ToList();

        if (requested.Any(permissionId => !validPermissionIds.Contains(permissionId)))
        {
            return BadRequest(new { message = "Uno o más permisos no existen." });
        }

        try
        {
            await _roleRepo.SetPermissionsAsync(id, requested);
            var updated = await _roleRepo.GetByIdWithPermissionsAsync(id);
            return Ok(updated is null ? null : MapToDto(updated));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static RoleResponseDto MapToDto(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description,
        IsActive = role.IsActive,
        Permissions = role.RolePermissions
            .Select(rolePermission => new PermissionSummaryDto
            {
                Id = rolePermission.Permission.Id,
                Code = rolePermission.Permission.Code,
                Description = rolePermission.Permission.Description
            })
            .OrderBy(permission => permission.Code)
            .ToList()
    };
}
