using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Friends;

public class SendFriendRequestDto
{
    [Required]
    public Guid ReceiverId { get; set; }
    public string? Message { get; set; }
}