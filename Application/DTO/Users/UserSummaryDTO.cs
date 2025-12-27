using Application.DTO.Friends;
using Domain.Entities;

namespace Application.DTO.Users;

public class UserSummaryDto 
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string AvatarUrl { get; set; }
    public string  Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Bio { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastActive { get; set; }
    
    public ICollection<FriendDto> Friends { get; set; }
}