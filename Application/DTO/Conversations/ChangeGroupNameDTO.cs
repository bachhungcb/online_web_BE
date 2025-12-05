using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Conversations;

public class ChangeGroupNameDto
{
    [Required]
    [MaxLength(100)]
    public string NewGroupName { get; set; }
}