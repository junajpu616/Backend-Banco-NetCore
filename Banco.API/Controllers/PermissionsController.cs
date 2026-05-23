namespace Banco.API.Controllers;

using Banco.Aplicacion.DTOs.Permissions;
using Banco.Aplicacion.Repositorios;
using Banco.Dominio.Constantes;
using Banco.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.RbacRead)]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionRepository _permissionRepo;

    public PermissionsController(IPermissionRepository permissionRepo)
    {
        _permissionRepo = permissionRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _permissionRepo.GetAllAsync();
        return Ok(permissions.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var permission = await _permissionRepo.GetByIdAsync(id);
        return permission is null ? NotFound() : Ok(MapToDto(permission));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.RbacManage)]
    public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            return BadRequest(new { message = "El código del permiso es requerido." });
        }

        if (await _permissionRepo.GetByCodeAsync(dto.Code) is not null)
        {
            return BadRequest(new { message = $"El permiso '{dto.Code}' ya existe." });
        }

        var permission = await _permissionRepo.AddAsync(new Permission
        {
            Id = Guid.NewGuid(),
            Code = dto.Code.Trim(),
            Description = dto.Description.Trim()
        });

        return CreatedAtAction(nameof(GetById), new { id = permission.Id }, MapToDto(permission));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.RbacManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePermissionDto dto)
    {
        var permission = await _permissionRepo.GetByIdAsync(id);
        if (permission is null)
        {
            return NotFound();
        }

        permission.Code = dto.Code.Trim();
        permission.Description = dto.Description.Trim();

        try
        {
            await _permissionRepo.UpdateAsync(permission);
            return Ok(MapToDto(permission));
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { message = "No se pudo actualizar el permiso." });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.RbacManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var permission = await _permissionRepo.GetByIdAsync(id);
        if (permission is null)
        {
            return NotFound();
        }

        await _permissionRepo.DeleteAsync(permission);
        return NoContent();
    }

    private static PermissionSummaryDto MapToDto(Permission permission) => new()
    {
        Id = permission.Id,
        Code = permission.Code,
        Description = permission.Description
    };
}
