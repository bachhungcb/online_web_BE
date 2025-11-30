using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Conversations;

public class CreateGroupConversationDto
{
    [Required]
    public string GroupName { get; set; }
    
    [Required]
    public List<Guid> MemberIds { get; set; } // Danh sách ID các thành viên muốn mời
}