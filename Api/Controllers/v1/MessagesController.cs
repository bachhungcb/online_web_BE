using Api.Hubs;
using Application.DTO.Conversations;
using Application.DTO.Messages;
using Application.Features.ConversationFeatures.Queries;
using Application.Features.MessageFeatures.Commands;
using Application.Features.MessageFeatures.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Api.Controllers.v1;

public class MessagesController : BaseApiController
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator, IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var senderId = CurrentUserId; // Lấy từ Token

        MessageSentResultDto result;
        // 1. Lưu vào DB qua MediatR
        var command = new SendMessageCommand(senderId, dto.ConversationId, dto.Content,dto.MediaUrls, dto.MessageType);
        try
        {
            result = await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        // 2. Gửi Real-time qua SignalR
        // Gửi sự kiện "ReceiveMessage" đến group có tên là ConversationId
        await _hubContext.Clients.Group(dto.ConversationId.ToString())
            .SendAsync("ReceiveMessage", new
            {
                SenderId = senderId,
                senderAvatarUrl =  result.SenderAvatarUrl,
                receiverId = result.ReceiverId,
                receiverUserName = result.ReceiverUserName,
                ReceiverAvartarUrl =  result.ReceiverAvatarUrl,
                content = dto.Content,
                mediaUrls = dto.MediaUrls,
                type = dto.MessageType,
                Timestamp = result.CreatedAt,
                conversationId = dto.ConversationId
            });
        
        return Ok(new
        {
            message = "Sent successfully",
            content = dto.Content,
            CreatedAt = DateTime.UtcNow,
        });
    }

    /// <summary>
    /// Lấy lịch sử tin nhắn của một cuộc trò chuyện (Có phân trang)
    /// </summary>
    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        // Tạo Query với CurrentUserId để check bảo mật
        var query = new GetMessagesByConversationIdQuery(
            conversationId,
            currentUserId,
            pageNumber,
            pageSize
        );
        
       
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("react")]
    public async Task<IActionResult> ReactToMessage([FromBody] ReactToMessageDto dto)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var command = new ReactToMessageCommand(dto.MessageId, currentUserId, dto.ReactionType);
    
        try 
        {
            var result = await _mediator.Send(command);
            return Ok(new { message = result });
        }
        catch (UnauthorizedAccessException) 
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}