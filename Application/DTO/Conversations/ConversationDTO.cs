using Application.DTO.Users;

namespace Application.DTO.Conversations;

public class ConversationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } // Tên hiển thị (Tên bạn bè hoặc Tên nhóm)
    public string AvatarUrl { get; set; } // Avatar hiển thị
    public string LastMessageContent { get; set; }
    public Guid LastMessageSenderId { get; set; }
    public string LastMessageSenderName { get; set; }
    public string LassMessageSenderAvatarUrl { get; set; }
    public DateTime LastMessageTime { get; set; }
    public bool IsRead { get; set; } // Đã đọc tin nhắn mới nhất chưa?
    
    public List<UserSummaryDto> Participants { get; set; }
}