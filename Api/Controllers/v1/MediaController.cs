using Application.DTO.Media;
using Application.Features.MediaFeatures.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.v1;

[ApiVersion("1.0")]
[Authorize]
public class MediaController : BaseApiController
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] //Limit 10MB
    public async Task<IActionResult> Upload([FromForm] UploadMediaDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            return BadRequest(new { message = "FILE can NOT be empty" });
        }
        
        // Folder có thể là "chat-images", "avatars", v.v.
        var command = new UploadMediaCommand(dto.File, "chat-attachments");
        
        try 
        {
            var url = await _mediator.Send(command);
            return Ok(new { Url = url });
        }
        catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}