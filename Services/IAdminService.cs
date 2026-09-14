using WebApi.NetCore.Dtos;
using WebApi.NetCore.Models;

namespace WebApi.NetCore.Services;

public interface IAdminService
{
    Task<PaginationResult<UserDto>> GetUsersAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserRoleAsync(int id, string role, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default);
}
