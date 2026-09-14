namespace WebApi.NetCore.Dtos;

public class LoginResponse
{
    public UserDto User { get; set; } = new();
    public TokenResponse Tokens { get; set; } = new();
}
