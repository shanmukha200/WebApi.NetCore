using System.ComponentModel.DataAnnotations;
using WebApi.NetCore.Constants;
using WebApi.NetCore.Data;
using WebApi.NetCore.Data.Repositories;
using WebApi.NetCore.Dtos;
using WebApi.NetCore.Models;
using WebApi.NetCore.Utilities;

namespace WebApi.NetCore.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenService jwtTokenService,
    JwtSettings jwtSettings) : IAuthService
{
    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingByUsername = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingByUsername is not null)
        {
            throw new ValidationException("Username already exists.");
        }

        var existingByEmail = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
        {
            throw new ValidationException("Email already exists.");
        }

        var newUser = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = AppConstants.Roles.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        newUser.Id = await userRepository.CreateAsync(newUser, cancellationToken);
        return MapToDto(newUser);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = request.UsernameOrEmail.Contains('@')
            ? await userRepository.GetByEmailAsync(request.UsernameOrEmail.Trim(), cancellationToken)
            : await userRepository.GetByUsernameAsync(request.UsernameOrEmail.Trim(), cancellationToken);

        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        var tokens = jwtTokenService.GenerateTokens(user);

        await refreshTokenRepository.CreateAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return new LoginResponse
        {
            User = MapToDto(user),
            Tokens = tokens
        };
    }

    public async Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken.Trim(), cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (existingToken.IsRevoked || existingToken.ExpiryDate <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User is inactive.");
        }

        var newTokens = jwtTokenService.GenerateTokens(user);
        var rotated = await refreshTokenRepository.RotateAsync(existingToken.Id, new RefreshToken
        {
            UserId = user.Id,
            Token = newTokens.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        if (!rotated)
        {
            throw new UnauthorizedAccessException("Refresh token is no longer valid.");
        }

        return newTokens;
    }

    public async Task LogoutAsync(int userId, RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken.Trim(), cancellationToken);

        if (token is not null && token.UserId == userId)
        {
            await refreshTokenRepository.RevokeAsync(token.Id, cancellationToken);
        }
    }

    public async Task<UserDto?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var byUsername = await userRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (byUsername is not null && byUsername.Id != userId)
        {
            throw new ValidationException("Username is already used by another user.");
        }

        var byEmail = await userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (byEmail is not null && byEmail.Id != userId)
        {
            throw new ValidationException("Email is already used by another user.");
        }

        await userRepository.UpdateProfileAsync(userId, request.Username.Trim(), request.Email.Trim(), cancellationToken);

        user.Username = request.Username.Trim();
        user.Email = request.Email.Trim();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (!PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ValidationException("Current password is incorrect.");
        }

        if (PasswordHasher.Verify(request.NewPassword, user.PasswordHash))
        {
            throw new ValidationException("New password must be different from current password.");
        }

        var updated = await userRepository.UpdatePasswordAsync(userId, PasswordHasher.Hash(request.NewPassword), cancellationToken);
        if (!updated)
        {
            throw new KeyNotFoundException("User not found.");
        }

        await refreshTokenRepository.RevokeAllForUserAsync(userId, cancellationToken);
        await refreshTokenRepository.DeleteExpiredAsync(cancellationToken);
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
