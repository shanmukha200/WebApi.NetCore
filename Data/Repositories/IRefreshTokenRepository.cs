using WebApi.NetCore.Models;

namespace WebApi.NetCore.Data.Repositories;

public interface IRefreshTokenRepository
{
    Task<int> CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(int id, CancellationToken cancellationToken = default);
    Task<int> RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
