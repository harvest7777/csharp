using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SwagApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MeController : ControllerBase
{
    [Authorize]
    [HttpGet(Name = "GetMe")]
    public ActionResult<User> Get()
    {
        User? user = HttpContext.Items["CurrentUser"] as User;
        if (user == null)
            return Unauthorized();
        return Ok(user);
    }
}