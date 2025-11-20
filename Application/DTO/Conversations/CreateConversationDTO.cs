using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Conversations;

public class CreateConversationDto
{
    [Required]
    public Guid ReceiverId { get; set; }
}