using System.ComponentModel.DataAnnotations;

namespace WebApi.NetCore.Dtos;

public class UpdateUserRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
