using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;

namespace SwagApi.Services;

public class UserResolver : IUserResolver
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private User? _resolvedUser;

    public UserResolver(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<User?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (_resolvedUser is not null)
        {
            return _resolvedUser;
        }

        var auth0Id = GetAuth0Id();
        if (string.IsNullOrEmpty(auth0Id))
        {
            return null;
        }

        _resolvedUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Auth0Id == auth0Id, ct);

        if (_resolvedUser is null)
        {
            var email = GetEmail();
            _resolvedUser = new User
            {
                Auth0Id = auth0Id,
                Email = email ?? auth0Id
            };
            _db.Users.Add(_resolvedUser);
            await _db.SaveChangesAsync(ct);
        }

        return _resolvedUser;
    }

    private string? GetAuth0Id()
    {
        return GetClaimValue(ClaimTypes.NameIdentifier)
            ?? GetClaimValue("sub");
    }

    private string? GetEmail()
    {
        return GetClaimValue(ClaimTypes.Email)
            ?? GetClaimValue("email");
    }

    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }
}
