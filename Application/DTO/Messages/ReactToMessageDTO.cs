using Domain.Entities;

namespace Application.DTO.Messages;

public class ReactToMessageDto
{
    public Guid MessageId { get; set; }
    public ReactionType ReactionType { get; set; }
}