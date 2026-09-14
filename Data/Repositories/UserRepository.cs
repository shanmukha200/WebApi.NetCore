using WebApi.NetCore.Models;

namespace WebApi.NetCore.Data.Repositories;

public class UserRepository(IDataAccess dataAccess) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => dataAccess.QuerySingleOrDefaultAsync<User>("""
            SELECT Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt
            FROM Users
            WHERE Id = @Id;
            """, new { Id = id }, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => dataAccess.QuerySingleOrDefaultAsync<User>("""
            SELECT Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt
            FROM Users
            WHERE Username = @Username;
            """, new { Username = username }, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => dataAccess.QuerySingleOrDefaultAsync<User>("""
            SELECT Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt
            FROM Users
            WHERE Email = @Email;
            """, new { Email = email }, cancellationToken);

    public Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
        => dataAccess.ExecuteScalarAsync<int>("""
            INSERT INTO Users (Username, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Username, @Email, @PasswordHash, @Role, @IsActive, @CreatedAt, @UpdatedAt);
            """, user, cancellationToken);

    public async Task<(IReadOnlyCollection<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var offset = (pageNumber - 1) * pageSize;
        var users = (await dataAccess.QueryAsync<User>("""
            SELECT Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt
            FROM Users
            ORDER BY Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """, new { Offset = offset, PageSize = pageSize }, cancellationToken)).ToArray();

        var total = await dataAccess.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users;", cancellationToken: cancellationToken);
        return (users, total);
    }

    public async Task<bool> UpdateRoleAsync(int id, string role, CancellationToken cancellationToken = default)
        => await dataAccess.ExecuteAsync("UPDATE Users SET Role = @Role, UpdatedAt = @UpdatedAt WHERE Id = @Id;",
            new { Id = id, Role = role, UpdatedAt = DateTime.UtcNow }, cancellationToken) > 0;

    public async Task<bool> UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        => await dataAccess.ExecuteAsync("UPDATE Users SET IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id;",
            new { Id = id, IsActive = isActive, UpdatedAt = DateTime.UtcNow }, cancellationToken) > 0;

    public async Task<bool> UpdateProfileAsync(int id, string username, string email, CancellationToken cancellationToken = default)
        => await dataAccess.ExecuteAsync("UPDATE Users SET Username = @Username, Email = @Email, UpdatedAt = @UpdatedAt WHERE Id = @Id;",
            new { Id = id, Username = username, Email = email, UpdatedAt = DateTime.UtcNow }, cancellationToken) > 0;

    public async Task<bool> UpdatePasswordAsync(int id, string passwordHash, CancellationToken cancellationToken = default)
        => await dataAccess.ExecuteAsync("UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = @UpdatedAt WHERE Id = @Id;",
            new { Id = id, PasswordHash = passwordHash, UpdatedAt = DateTime.UtcNow }, cancellationToken) > 0;

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => await dataAccess.ExecuteAsync("UPDATE Users SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id;",
            new { Id = id, UpdatedAt = DateTime.UtcNow }, cancellationToken) > 0;

    public Task<int> CountByRoleAsync(string role, CancellationToken cancellationToken = default)
        => dataAccess.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users WHERE Role = @Role;", new { Role = role }, cancellationToken);

    public Task<int> CountAllAsync(CancellationToken cancellationToken = default)
        => dataAccess.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users;", cancellationToken: cancellationToken);

    public Task<int> CountByStatusAsync(bool isActive, CancellationToken cancellationToken = default)
        => dataAccess.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users WHERE IsActive = @IsActive;", new { IsActive = isActive }, cancellationToken);
}
