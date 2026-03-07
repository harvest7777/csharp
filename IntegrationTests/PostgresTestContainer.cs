using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;

namespace IntegrationTests;

public class PostgresTestContainer : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public DbContextOptions<ApplicationDbContext> Options { get; private set; } = null!;

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

        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new ApplicationDbContext(Options);
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync()
        => _container.DisposeAsync().AsTask();
}