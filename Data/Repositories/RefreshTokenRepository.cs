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

    public async Task<bool> RotateAsync(int currentTokenId, RefreshToken newToken, CancellationToken cancellationToken = default)
        => await dataAccess.ExecuteScalarAsync<bool>("""
            BEGIN TRANSACTION;
            UPDATE RefreshTokens
            SET IsRevoked = 1
            WHERE Id = @CurrentTokenId AND IsRevoked = 0 AND ExpiryDate > @UtcNow;

            IF @@ROWCOUNT = 0
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT CAST(0 AS BIT);
                RETURN;
            END

            INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, IsRevoked, CreatedAt)
            VALUES (@UserId, @Token, @ExpiryDate, 0, @CreatedAt);

            COMMIT TRANSACTION;
            SELECT CAST(1 AS BIT);
            """, new
        {
            CurrentTokenId = currentTokenId,
            UtcNow = DateTime.UtcNow,
            newToken.UserId,
            newToken.Token,
            newToken.ExpiryDate,
            newToken.CreatedAt
        }, cancellationToken);

    public Task<int> RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default)
        => dataAccess.ExecuteAsync("UPDATE RefreshTokens SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0;", new { UserId = userId }, cancellationToken);

    public Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
        => dataAccess.ExecuteAsync("DELETE FROM RefreshTokens WHERE ExpiryDate < @UtcNow;", new { UtcNow = DateTime.UtcNow }, cancellationToken);
}
