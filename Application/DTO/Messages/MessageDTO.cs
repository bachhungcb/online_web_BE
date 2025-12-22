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
    // Group reaction theo Type để đếm số lượng: "Like": 5, "Love": 2
    public Dictionary<string, int> ReactionsCount { get; set; } 
    // Hoặc chi tiết user nào thả gì (nếu cần hiển thị tooltip)
    public List<ReactionDetailDto> Reactions { get; set; }
}

public class ReactionDetailDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string ReactionType { get; set; }
}