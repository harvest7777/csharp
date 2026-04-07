namespace SwagApi;

public class RequestAuthMiddleware
{
    private readonly RequestDelegate _next;

    public RequestAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserResolver userResolver)
    {
        var user = await userResolver.GetOrCreateUser();
        context.Items["CurrentUser"] = user;
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
