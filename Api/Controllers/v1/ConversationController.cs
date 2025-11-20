using Application.DTO.Conversations;
using Application.Features.ConversationFeatures.Commands;
using Application.Features.ConversationFeatures.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.v1;


[ApiVersion("1.0")]
[Authorize] // Bắt buộc đăng nhập
public class ConversationController : BaseApiController
{
    private readonly IMediator _mediator;

    public ConversationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tạo cuộc trò chuyện mới hoặc lấy ID cuộc trò chuyện cũ nếu đã tồn tại
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDirectConversation([FromBody] CreateConversationDto dto)
    {
        var senderId = CurrentUserId;
        if (senderId == Guid.Empty) return Unauthorized();

        var command = new CreateConversationCommand(senderId, dto.ReceiverId);

        try
        {
            var conversationId = await _mediator.Send(command);
            return Ok(new { conversationId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Lấy danh sách các cuộc trò chuyện của người dùng hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConversationList()
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var query = new GetConversationListQuery(currentUserId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}