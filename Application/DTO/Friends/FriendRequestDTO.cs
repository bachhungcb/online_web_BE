namespace Application.DTO.Friends;

public class FriendRequestDto
{
    public Guid Id { get; set; } // ID của FriendRequest
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}