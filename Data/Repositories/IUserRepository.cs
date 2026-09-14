using WebApi.NetCore.Models;

namespace WebApi.NetCore.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<(IReadOnlyCollection<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> UpdateRoleAsync(int id, string role, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(int id, string username, string email, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordAsync(int id, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(bool isActive, CancellationToken cancellationToken = default);
}
