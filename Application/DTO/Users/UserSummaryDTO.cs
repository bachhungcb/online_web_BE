namespace Application.DTO.Users;

public class UserSummaryDto 
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string AvatarUrl { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastActive { get; set; }
}