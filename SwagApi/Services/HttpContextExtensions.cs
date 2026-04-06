namespace SwagApi.Services;

public static class HttpContextExtensions
{
    public static User? GetCurrentUser(this HttpContext context)
    {
        return context.Items["CurrentUser"] as User;
    }

    public static User? GetCurrentUser(this HttpRequest request)
    {
        return request.HttpContext.GetCurrentUser();
    }
}
