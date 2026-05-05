using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookTracker.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected Guid UserId
    {
        get
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
        }
    }

    protected string UserRole
    {
        get
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "User";
        }
    }

    protected string UserEmail
    {
        get
        {
            return User.FindFirstValue(ClaimTypes.Email) ?? "System";
        }
    }
}
