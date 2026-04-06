using System.Security.Claims;

namespace Middleware.Example;

public class RequestAuthMiddleware
{
    private readonly RequestDelegate _next;

    public RequestAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sub = context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }
}

public static class RequestAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestAuth(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestAuthMiddleware>();
    }
}