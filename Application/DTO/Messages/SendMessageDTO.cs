using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Messages;

public class SendMessageDto
{
    [Required]
    public Guid SenderId { get; set; }
    [Required]
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
}