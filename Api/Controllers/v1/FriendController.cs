using Application.DTO.Friends;
using Application.Features.FriendFeatures.Commands;
using Application.Features.FriendFeatures.Queries;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class FriendController : BaseApiController
{
    private readonly IMediator _mediator;

    public FriendController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("requests")]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto dto) 
    {
        var senderId = CurrentUserId;

        if (senderId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new SendFriendRequestCommand(
            senderId,
            dto.ReceiverId,
            dto.Message);

        try
        {
            var requestId = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetFriendRequestById), new { id = requestId }, requestId);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetFriendRequestById(Guid id)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var query = new GetFriendRequestByIdQuery(id, currentUserId);
        try
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    
    [HttpPost("requests/{id:guid}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(Guid id)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var command = new AcceptFriendRequestCommand(id, currentUserId);
        try
        {
            var result = await Mediator.Send(command);
            if (result)
                return Ok(new { message = "Accept friend request successfully" });
            else
                return BadRequest(new { message = "Can NOT accept friend request " });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("requests/{id:guid}")]
    public async Task<IActionResult> DeleteFriendRequest(Guid id)
    {
        var currentUserId = CurrentUserId;
        if (currentUserId == Guid.Empty) return Unauthorized();

        var command = new DeleteFriendRequestCommand(id, currentUserId);
        try
        {
            await Mediator.Send(command);
            return Ok(new { message = "Đã xóa/từ chối yêu cầu kết bạn." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetFriendList()
    {
        throw new NotImplementedException();
    }
}