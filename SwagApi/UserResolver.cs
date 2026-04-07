using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;

namespace SwagApi;

public class UserResolver
{
    private ApplicationDbContext db;
    private IHttpContextAccessor httpContextAccessor;

    public UserResolver(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        this.db = db;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<User> GetOrCreateUserId()
    {
        var auth0Id = GetAuth0Id() ?? throw new UnauthorizedAccessException("No Auth0 Id found in claims.");
        User foundUser = await db.Users.FirstOrDefaultAsync(x => x.Auth0Id == auth0Id);

        if (foundUser != null)
        {
            return foundUser;
        }

        User newUser = new User
        {
            Auth0Id = auth0Id,
        };
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return newUser;
    }

    private string? GetAuth0Id()
    {
        var sub = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return sub;
    }
}