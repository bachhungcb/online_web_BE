using Azure.Identity;

namespace Domain.Entities;

public class MessageReaction : BaseEntity
{
    public required Guid MessageId { get; set; }
    public Message Message { get; set; }
    
    public required Guid UserId { get; set; }
    public User User { get; set; }
    
    public required ReactionType ReactionType { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReactionType
{
    Like = 0,
    Dislike = 1,
    Haha = 2,
    Love = 3,
}