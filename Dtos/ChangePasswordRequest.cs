using System.ComponentModel.DataAnnotations;

namespace WebApi.NetCore.Dtos;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(200)]
    public string NewPassword { get; set; } = string.Empty;
}
