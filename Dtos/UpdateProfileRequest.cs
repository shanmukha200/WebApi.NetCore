using System.ComponentModel.DataAnnotations;

namespace WebApi.NetCore.Dtos;

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
}
