using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    /// <summary>
    /// Lấy UserId của người dùng đã được xác thực từ JWT Token.
    /// </summary>
    protected Guid CurrentUserId
    {
        get
        {
            var idValue = User.FindFirstValue("id");

            if (Guid.TryParse(idValue, out Guid userId))
            {
                return userId;
            }

            return Guid.Empty;
        }
    }

    protected string CurrentUserName
    {
        get
        {
            var UserName = User.FindFirstValue("UserName");

            if (UserName == null)
            {
                return string.Empty;
            }

            return UserName;
        }
    }
}