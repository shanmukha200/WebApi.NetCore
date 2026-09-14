using WebApi.NetCore.Dtos;

namespace WebApi.NetCore.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(int userId, RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
