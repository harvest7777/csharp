namespace IntegrationTests;

public class PostgresTestContainer
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public PostgresTestContainer()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

}