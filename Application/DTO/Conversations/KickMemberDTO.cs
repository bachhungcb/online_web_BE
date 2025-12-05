using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Conversations;

public class KickMemberDto
{
    [Required]
    public Guid MemberId { get; set; } // ID của người bị kick
}