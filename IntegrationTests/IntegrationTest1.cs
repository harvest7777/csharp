using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using SwagApi;
using SwagApi.Data;
using SwagApi.DTOs;

namespace IntegrationTests;

public class WeatherForecastIntegrationTests 
    : IClassFixture<PostgresTestContainer>, IAsyncLifetime
{
    private readonly PostgresTestContainer _fixture;
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    public WeatherForecastIntegrationTests(PostgresTestContainer fixture)
    {
        _fixture = fixture;
    }

    // Runs before each test
    public async Task InitializeAsync()
    {
        // It's important to reset the database because us tests are atomic and
        // fully idempotent. 
        await _fixture.ResetDatabaseAsync();
        
        // It's really important we re-initialize the HTTP client because HTTP clients
        // are stateful and can carry headers and cookies in between tests.
        _factory = new CustomWebApplicationFactory(_fixture.ConnectionString);
        _client = _factory.CreateClient();
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

    [Fact]
    public async Task Post_Should_Insert_One_Record()
    {
        var newWeatherDto = new WeatherForecastDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 20,
            Summary = "test"
        };
        var response = await _client.PostAsJsonAsync(
            "/weatherforecast",
            newWeatherDto);
        
        // This should have inserted the new weather into the database. 
        await using var context = new ApplicationDbContext(_fixture.Options);
        var count = await context.WeatherForecasts.CountAsync();
        Assert.Equal(1, count);
        var weatherForecast = await context.WeatherForecasts.SingleAsync(w => w.Summary == "test");
        var inserted = await context.WeatherForecasts
            .SingleAsync(w => w.Summary == "test");

        // The inserted weather forecast should be the same one that we made in the post request.
        Assert.Equal(newWeatherDto.Date, inserted.Date);
        Assert.Equal(newWeatherDto.TemperatureC, inserted.TemperatureC);
        Assert.Equal(newWeatherDto.Summary, inserted.Summary);
    }
    
}