namespace SwagApi;

public class User
{
    public int Id { get; set; }
    public required string Auth0Id { get; set; }
    public required string Email { get; set; }
}