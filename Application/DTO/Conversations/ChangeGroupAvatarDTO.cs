using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Conversations;

public class ChangeGroupAvatarDto
{
    [Required]
    public string Avatar { get; set; }
}