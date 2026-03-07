using Microsoft.EntityFrameworkCore;
using SwagApi;
using SwagApi.Data;

namespace IntegrationTests;
public class WeatherForecastIntegrationTests 
    : IClassFixture<PostgresTestContainer>
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public WeatherForecastIntegrationTests(PostgresTestContainer fixture)
    {
        _options = fixture.Options;
    }

    [Fact]
    public async Task Should_Insert_WeatherForecast()
    {
        await using var context = new ApplicationDbContext(_options);

        var forecast = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 30,
            Summary = "Hot"
        };

        context.WeatherForecasts.Add(forecast);
        await context.SaveChangesAsync();

        var saved = await context.WeatherForecasts
            .FirstOrDefaultAsync(x => x.Summary == "Hot");

        Assert.NotNull(saved);
    }
}