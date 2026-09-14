using WebApi.NetCore.Models;

namespace WebApi.NetCore.Data.Repositories;

public class RefreshTokenRepository(IDataAccess dataAccess) : IRefreshTokenRepository
{
    public Task<int> CreateAsync(RefreshToken token, CancellationToken cancellationToken = default)
        => dataAccess.ExecuteScalarAsync<int>("""
            INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, IsRevoked, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Token, @ExpiryDate, @IsRevoked, @CreatedAt);
            """, token, cancellationToken);

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => dataAccess.QuerySingleOrDefaultAsync<RefreshToken>("""
            SELECT Id, UserId, Token, ExpiryDate, IsRevoked, CreatedAt
            FROM RefreshTokens
            WHERE Token = @Token;
            """, new { Token = token }, cancellationToken);

    public async Task<bool> RevokeAsync(int id, CancellationToken cancellationToken = default)
        => await dataAccess.ExecuteAsync("UPDATE RefreshTokens SET IsRevoked = 1 WHERE Id = @Id;", new { Id = id }, cancellationToken) > 0;

    public Task<int> RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default)
        => dataAccess.ExecuteAsync("UPDATE RefreshTokens SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0;", new { UserId = userId }, cancellationToken);

    public Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
        => dataAccess.ExecuteAsync("DELETE FROM RefreshTokens WHERE ExpiryDate < @UtcNow OR IsRevoked = 1;", new { UtcNow = DateTime.UtcNow }, cancellationToken);
}
