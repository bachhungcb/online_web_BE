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
}