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
    /// Create random conversation
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRandomConversation()
    {
        var senderId = CurrentUserId;
        if (senderId == Guid.Empty) return Unauthorized();

        var command = new CreateRandomConversationCommand(senderId);

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

        var command = new CreateGroupConversationCommand(senderId, dto.GroupName, dto.GroupAvatar, dto.MemberIds);

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
    /// <param name="conversationId">ID của cuộc trò chuyện (Nhóm)</param>
    /// <param name="dto">Danh sách ID thành viên cần thêm</param>
    [HttpPost("{conversationId:guid}/members")]
    public async Task<IActionResult> AddMemberToGroup([FromRoute] Guid conversationId, [FromBody] AddMemberToGroupDto dto)
    {
        var currentUserId = CurrentUserId; // Lấy từ Token (BaseApiController)
        if (currentUserId == Guid.Empty) return Unauthorized();

        // Map từ Route + Body + Token sang Command
        var command = new AddMemberToGroupCommand(
            ConversationId: conversationId,
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
    
    /// <summary>
    /// Rời khỏi nhóm chat
    /// </summary>
    [HttpDelete("{groupId:guid}/leave")]
    public async Task<IActionResult> LeaveGroup([FromRoute] Guid groupId)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var command = new LeaveGroupCommand(groupId, currentUserId);

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Left group successfully" });
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
    
    // 1. API KICK MEMBER
    [HttpPost("{memberId:guid}/kick")]
    public async Task<IActionResult> KickMember([FromRoute] Guid memberId, [FromBody] KickMemberDto dto)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var command = new KickMemberCommand(memberId, currentUserId, dto.MemberId);

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Member kicked successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid(); // Trả về 403 nếu không phải Admin
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute]Guid conversationId)
    {
        await _mediator.Send(new MarkConversationAsReadCommand(conversationId, CurrentUserId));
        return Ok(new { message = "Marked as read" });
    }

    // 2. API CHANGE GROUP NAME
    [HttpPut("{groupId:guid}/group/name")]
    public async Task<IActionResult> ChangeGroupName([FromRoute] Guid groupId, [FromBody] ChangeGroupNameDto dto)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var command = new ChangeGroupNameCommand(groupId, currentUserId, dto.NewGroupName);

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Group name updated successfully", newName = dto.NewGroupName });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    
    [HttpPut("{groupId:guid}/group/avatar")]
    public async Task<IActionResult> ChangeGroupAvatar([FromRoute] Guid groupId, [FromBody] ChangeGroupAvatarDto dto)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var command = new ChangeGroupAvatarCommand(groupId, currentUserId, dto.Avatar);

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Group avatar updated successfully", newAvatar = dto.Avatar });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}