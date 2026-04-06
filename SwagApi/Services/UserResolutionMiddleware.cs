namespace SwagApi.Services;

public class UserResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public UserResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserResolver userResolver)
    {
        Console.WriteLine($"[USER MIDDLEWARE] Authenticated: {context.User.Identity?.IsAuthenticated}");
        Console.WriteLine($"[USER MIDDLEWARE] Identity.Name: {context.User.Identity?.Name}");
        
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userResolver.GetCurrentUserAsync();
            Console.WriteLine($"[USER MIDDLEWARE] Resolved user: {user?.Email}");
            context.Items["CurrentUser"] = user;
        }

        await _next(context);
    }
}

public static class UserResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseUserResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserResolutionMiddleware>();
    }
}
