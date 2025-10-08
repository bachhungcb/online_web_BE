using Api.Filter;
using Api.Wrappers;
using Application.Features.UserFeatures.Commands;
using Application.Features.UserFeatures.Queries;
using Asp.Versioning;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.v1;

[ApiVersion("1.0")]
public class UserController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        return Ok(new Response<Guid>(await Mediator.Send(command)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] PaginationFilter filter)
    {
        var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
        var response = await Mediator.Send(new GetAllUserQuery(validFilter.PageNumber, validFilter.PageSize));
        return Ok(new PagedResponse<IEnumerable<User>>(response, validFilter.PageNumber, validFilter.PageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        return Ok(await Mediator.Send(new GetUserByIdQuery { Id = id }));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        return Ok(await Mediator.Send(new DeleteUserByIdCommand { Id = id }));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        return Ok(await Mediator.Send(command));
    }
}