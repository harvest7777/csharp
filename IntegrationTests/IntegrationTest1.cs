using Microsoft.EntityFrameworkCore;
using SwagApi;
using SwagApi.Data;
namespace IntegrationTests;

public class WeatherForecastIntegrationTests 
    : IClassFixture<PostgresTestContainer>, IAsyncLifetime
{
    private readonly PostgresTestContainer _fixture;

    public WeatherForecastIntegrationTests(PostgresTestContainer fixture)
    {
        _fixture = fixture;
    }

    // Runs before each test
    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }
    
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Should_Insert_One_Record()
    {
        await using var context = new ApplicationDbContext(_fixture.Options);

        context.WeatherForecasts.Add(new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 20,
            Summary = "Mild"
        });

        await context.SaveChangesAsync();

        var count = await context.WeatherForecasts.CountAsync();

        Assert.Equal(1, count);
    }
    [Fact]
    public async Task Weather_Should_Be_Empty_Initially()
    {
        await using var context = new ApplicationDbContext(_fixture.Options);

        var count = await context.WeatherForecasts.CountAsync();

        Assert.Equal(0, count);
    }
}