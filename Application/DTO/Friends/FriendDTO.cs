namespace Application.DTO.Friends;

public class FriendDto
{
    public Guid Id { get; set; } 
    public Guid FriendshipId { get; set; } // ID của bản ghi Friend
    public Guid FriendUserId { get; set; } // ID của người bạn
    public string FriendUserName { get; set; }
    public string FriendAvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } // Ngày kết bạn
}