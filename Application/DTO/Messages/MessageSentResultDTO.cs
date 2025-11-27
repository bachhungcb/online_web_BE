namespace Application.DTO.Messages;

public class MessageSentResultDto
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string ReceiverUserName { get; set; }
    public string ReceiverAvatarUrl { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    
}