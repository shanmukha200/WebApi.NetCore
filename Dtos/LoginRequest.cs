using System.ComponentModel.DataAnnotations;

namespace WebApi.NetCore.Dtos;

public class LoginRequest
{
    [Required]
    [MaxLength(100)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;
}
