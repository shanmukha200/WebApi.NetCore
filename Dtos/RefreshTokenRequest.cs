using System.ComponentModel.DataAnnotations;

namespace WebApi.NetCore.Dtos;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
