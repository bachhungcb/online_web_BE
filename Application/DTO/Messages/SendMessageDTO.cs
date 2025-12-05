using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace Application.DTO.Messages;

public class SendMessageDto
{
    [Required]
    public Guid SenderId { get; set; }
    [Required]
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
    public List<string>? MediaUrls { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
}