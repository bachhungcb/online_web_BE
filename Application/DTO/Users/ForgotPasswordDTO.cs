using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Users;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}