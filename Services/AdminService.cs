using System.ComponentModel.DataAnnotations;
using WebApi.NetCore.Constants;
using WebApi.NetCore.Data.Repositories;
using WebApi.NetCore.Dtos;
using WebApi.NetCore.Models;

namespace WebApi.NetCore.Services;

public class AdminService(IUserRepository userRepository) : IAdminService
{
    public async Task<PaginationResult<UserDto>> GetUsersAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var (users, totalCount) = await userRepository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
        return new PaginationResult<UserDto>
        {
            Items = users.Select(MapToDto).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapToDto(user);
    }

    public Task<bool> UpdateUserStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => userRepository.UpdateStatusAsync(id, isActive, cancellationToken);

    public async Task<bool> UpdateUserRoleAsync(int id, string role, CancellationToken cancellationToken = default)
    {
        if (!AppConstants.ValidRoles.Contains(role))
        {
            throw new ValidationException($"Role must be one of: {string.Join(", ", AppConstants.ValidRoles)}.");
        }

        return await userRepository.UpdateRoleAsync(id, role, cancellationToken);
    }

    public Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
        => userRepository.DeleteAsync(id, cancellationToken);

    public async Task<AdminStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await userRepository.CountAllAsync(cancellationToken);
        var activeUsers = await userRepository.CountByStatusAsync(true, cancellationToken);
        var inactiveUsers = await userRepository.CountByStatusAsync(false, cancellationToken);
        var adminUsers = await userRepository.CountByRoleAsync(AppConstants.Roles.Admin, cancellationToken);
        var managerUsers = await userRepository.CountByRoleAsync(AppConstants.Roles.Manager, cancellationToken);
        var standardUsers = await userRepository.CountByRoleAsync(AppConstants.Roles.User, cancellationToken);

        return new AdminStatsResponse
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            AdminUsers = adminUsers,
            ManagerUsers = managerUsers,
            StandardUsers = standardUsers
        };
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
