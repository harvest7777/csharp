using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;
using Respawn;
using Npgsql;

namespace IntegrationTests;

public class PostgresTestContainer : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    private Respawner _respawner = null!;
    private string _connectionString = null!;

    public DbContextOptions<ApplicationDbContext> Options { get; private set; } = null!;

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

        _connectionString = _container.GetConnectionString();

        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        // Apply migrations once
        await using (var context = new ApplicationDbContext(Options))
        {
            await context.Database.MigrateAsync();
        }

        // Create respawner AFTER migrations
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public Task DisposeAsync()
        => _container.DisposeAsync().AsTask();
}