using Api.Hubs;
using Application.DTO.Messages;
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

        // 1. Lưu vào DB qua MediatR
        var command = new SendMessageCommand(senderId, dto.ConversationId, dto.Content);
        try 
        {
            await Mediator.Send(command);
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
                Content = dto.Content, 
                Timestamp = DateTime.Now 
            });

        return Ok(new { message = "Sent successfully" });
    }
    
    /// <summary>
    /// Lấy lịch sử tin nhắn của một cuộc trò chuyện (Có phân trang)
    /// </summary>
    [HttpGet("{conversationId}")]
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
}