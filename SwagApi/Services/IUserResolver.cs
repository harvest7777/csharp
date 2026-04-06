namespace SwagApi.Services;

public interface IUserResolver
{
    Task<User?> GetCurrentUserAsync(CancellationToken ct = default);
}
