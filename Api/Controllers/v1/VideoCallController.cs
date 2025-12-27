using Application.Features.VideoCallFeatures.Command;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.v1;

[ApiVersion("1.0")]
public class VideoCallController :BaseApiController
{
    private readonly IMediator _mediator;
    public VideoCallController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVideoCall([FromBody] CreateVideoCallTokenCommand command)
    {
        try
        {
            var token = await _mediator.Send(command);
            return Ok(new {Token = token});
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }
    
    // API đồng bộ user thủ công (thường dùng cho Admin hoặc khi user update profile)
    [HttpPost("sync-user")]
    public async Task<IActionResult> SyncUser([FromBody] UpsertStreamUserCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new { Success = result, Message = "User synced with Stream provider." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // API xóa user (Hard Delete)
    [HttpDelete("user/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            await _mediator.Send(new DeleteStreamUserCommand(userId));
            return Ok(new { Message = "User deleted from Stream provider." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
}