using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.NetCore.Constants;
using WebApi.NetCore.Dtos;
using WebApi.NetCore.Models;
using WebApi.NetCore.Services;

namespace WebApi.NetCore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppConstants.Roles.Admin)]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<PaginationResult<UserDto>>> GetUsers([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
    {
        var users = await adminService.GetUsersAsync(request, cancellationToken);
        return Ok(users);
    }

    [HttpGet("users/{id:int}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await adminService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse { Message = "User not found." });
        }

        return Ok(user);
    }

    [HttpPut("users/{id:int}/role")]
    public async Task<ActionResult<ApiMessageResponse>> UpdateRole(int id, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var updated = await adminService.UpdateUserRoleAsync(id, request.Role.Trim(), cancellationToken);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse { Message = "User not found." });
        }

        return Ok(new ApiMessageResponse { Message = "User role updated." });
    }

    [HttpPut("users/{id:int}/status")]
    public async Task<ActionResult<ApiMessageResponse>> UpdateStatus(int id, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await adminService.UpdateUserStatusAsync(id, request.IsActive, cancellationToken);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse { Message = "User not found." });
        }

        return Ok(new ApiMessageResponse { Message = "User status updated." });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<ActionResult<ApiMessageResponse>> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var deleted = await adminService.DeleteUserAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ApiErrorResponse { Message = "User not found." });
        }

        return Ok(new ApiMessageResponse { Message = "User deleted." });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStats(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetStatsAsync(cancellationToken));
    }
}
