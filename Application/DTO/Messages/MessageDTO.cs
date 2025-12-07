using Domain.Entities;

namespace Application.DTO.Messages;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } // Để hiển thị tên người gửi
    public string SenderAvatarUrl { get; set; } // Để hiển thị avatar
    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverAvatarUrl { get; set; }
    public string Content { get; set; }
    public List<string> MediaUrls { get; set; }
    public MessageType MessageType { get; set; }
    public DateTime CreatedAt { get; set; }
}