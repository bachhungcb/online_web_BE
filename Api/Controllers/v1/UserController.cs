using Api.Filter;
using Api.Helpers;
using Api.Services;
using Api.Wrappers;
using Application.DTO.Users;
using Application.Features.UserFeatures.Commands;
using Application.Features.UserFeatures.Queries;
using Asp.Versioning;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.v1;

[ApiVersion("1.0")]
public class UserController : BaseApiController
{
    private readonly IUriService _uriService;
    private readonly IMediator _mediator;

    public UserController(IUriService uriService, IMediator mediator)
    {
        _uriService = uriService;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        try
        {
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }

        return Ok(new Response<Guid>(await _mediator.Send(command)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] PaginationFilter filter)
    {
        try
        {
            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            var response = (await _mediator.Send(new GetAllUserQuery(validFilter.PageNumber, validFilter.PageSize)))
                .ToList();
            var totalRecords = response.Count;
            var returnResponse =
                PaginationHelper.CreatePagedReponse<User>(response,
                    validFilter,
                    totalRecords,
                    _uriService,
                    route);
            return Ok(new { UserList = returnResponse });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet("list-id")]
    public async Task<IActionResult> GetListId([FromQuery] PaginationFilter filter)
    {
        try
        {
            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            var response = (await _mediator.Send(new GetListUserIdQuery())).ToList();
            var totalRecords = response.Count;
            return Ok(new { UserList = response, TotalRecords = totalRecords });
        }catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            return Ok(await _mediator.Send(new GetUserByIdQuery(id)));
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            return Ok(await _mediator.Send(new DeleteUserByIdCommand { Id = id }));
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var command = new UpdateUserCommand
            {
                Id = id,
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.Email,
                AvatarUrl = dto.AvatarUrl,
                Bio = dto.Bio,
                Phone = dto.Phone
            };
            return Ok(await _mediator.Send(command));
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