namespace SwagApi;

public class RequestAuthMiddleware
{
    private readonly RequestDelegate _next;

    public RequestAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync( HttpContext context, UserResolver userResolver)
    {
        await userResolver.GetOrCreateUserId();
        //
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