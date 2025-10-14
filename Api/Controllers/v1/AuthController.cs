using Api.Services;
using Application.Features.AuthFeatures.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.v1;

[ApiVersion("1.0")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IUriService _uriService;

    public AuthController(IUriService uriService, IMediator mediator)
    {
        _uriService = uriService;
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var token = await _mediator.Send(command);
        
        return Ok(new{Token = token});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var userId = await _mediator.Send(command);
        
        return Ok(new{UserId = userId});
    }
}