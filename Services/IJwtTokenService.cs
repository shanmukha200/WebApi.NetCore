using WebApi.NetCore.Dtos;
using WebApi.NetCore.Models;

namespace WebApi.NetCore.Services;

public interface IJwtTokenService
{
    TokenResponse GenerateTokens(User user);
}
