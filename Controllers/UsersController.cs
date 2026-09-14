using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.NetCore.Dtos;
using WebApi.NetCore.Services;

namespace WebApi.NetCore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IAuthService authService) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized(new ApiErrorResponse { Message = "Unauthorized." });
        }

        var user = await authService.GetProfileAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse { Message = "User not found." });
        }

        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized(new ApiErrorResponse { Message = "Unauthorized." });
        }

        var user = await authService.UpdateProfileAsync(userId, request, cancellationToken);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse { Message = "User not found." });
        }

        return Ok(user);
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiMessageResponse>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized(new ApiErrorResponse { Message = "Unauthorized." });
        }

        await authService.ChangePasswordAsync(userId, request, cancellationToken);
        return Ok(new ApiMessageResponse { Message = "Password changed successfully." });
    }
}
