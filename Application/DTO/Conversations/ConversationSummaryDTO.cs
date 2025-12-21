using Application.DTO.Users;

namespace Application.DTO.Conversations;

public class ConversationSummaryDto
{
    public Guid ConversationId { get; set; }
    
    public required UserSummaryDto Sender { get; set; }
    public required UserSummaryDto Receiver { get; set; }
}