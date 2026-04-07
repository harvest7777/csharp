using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        string Auth0Id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == Auth0Id);
        return Ok(user);
    }
}