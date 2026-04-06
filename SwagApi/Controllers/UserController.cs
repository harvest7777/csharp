using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwagApi.Services;

namespace SwagApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var user = HttpContext.GetCurrentUser();
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new { user.Id, user.Email });
    }
}
