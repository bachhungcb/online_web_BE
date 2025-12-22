using Application.DTO.Users;

namespace Application.DTO.Conversations;

public class ConversationSummaryDto
{
    public Guid ConversationId { get; set; }
    public string Name { get; set; } // Receiver Name
    public string AvatarUrl { get; set; } // Receiver's avt url
}