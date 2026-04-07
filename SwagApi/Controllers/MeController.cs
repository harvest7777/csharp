using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwagApi.Data;

namespace SwagApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MeController(ApplicationDbContext context)
    {
        _context = context;
    } 

    [Authorize]
    [HttpGet(Name = "GetMe")]
    public async Task<ActionResult<User>> Get()
    {
        User? user = HttpContext.Items["CurrentUser"] as User;
        if (user == null)
            return Unauthorized();
        return Ok(user);
    }
}