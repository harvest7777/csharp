using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;

namespace SwagApi;

public class UserResolver
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserResolver(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<User> GetOrCreateUser()
    {
        var auth0Id = GetAuth0Id() ?? throw new UnauthorizedAccessException("No Auth0 Id found in claims.");
        User foundUser = await _db.Users.FirstOrDefaultAsync(x => x.Auth0Id == auth0Id);

        if (foundUser != null)
        {
            return foundUser;
        }

        User newUser = new User
        {
            Auth0Id = auth0Id,
        };
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
        return newUser;
    }

    private string? GetAuth0Id()
    {
        var sub = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return sub;
    }
}
