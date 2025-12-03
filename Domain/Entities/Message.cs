namespace Domain.Entities;

public class Message : BaseEntity
{
    //Sender Foreignkey
    public required Guid SenderId {get; set;}
    public User Sender { get; set; }
    
    //Conversation Foreign Key
    public required Guid ConversationId {get; set;}
    public Conversation Conversation {get; set;}
    
    public string Content {get; set;}
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Text;
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    Video = 2,
    File = 3,
    System = 99
}