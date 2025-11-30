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
    /// Tạo nhóm chat mới
    /// </summary>
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupConversationDto dto)
    {
        var senderId = CurrentUserId; // Lấy từ Token
        if (senderId == Guid.Empty) return Unauthorized();

        var command = new CreateGroupConversationCommand(senderId, dto.GroupName, dto.MemberIds);

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
    /// Thêm thành viên vào nhóm chat
    /// </summary>
    /// <param name="id">ID của cuộc trò chuyện (Nhóm)</param>
    /// <param name="dto">Danh sách ID thành viên cần thêm</param>
    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMemberToGroup([FromRoute] Guid id, [FromBody] AddMemberToGroupDto dto)
    {
        var currentUserId = CurrentUserId; // Lấy từ Token (BaseApiController)
        if (currentUserId == Guid.Empty) return Unauthorized();

        // Map từ Route + Body + Token sang Command
        var command = new AddMemberToGroupCommand(
            ConversationId: id,
            NewMemberIds: dto.MemberIds,
            RequestorId: currentUserId
        );

        try
        {
            // Gửi sang Handler xử lý
            await _mediator.Send(command);
            
            // Trả về 200 OK nếu thành công
            return Ok(new { message = "Add members successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            // Trả về 403 nếu không có quyền (không phải thành viên nhóm)
            return Forbid();
        }
        catch (Exception ex)
        {
            // Trả về 400 cho các lỗi logic khác (như Group không tồn tại, ID rác...)
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