using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Conversations;

public class AddMemberToGroupDto
{
    [Required]
    public List<Guid> MemberIds { get; set; }
}