using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using SwagApi;
using SwagApi.Data;
using SwagApi.DTOs;
using System.Text;
using System.Net;
using Microsoft.EntityFrameworkCore;

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

    [Fact]
    public async Task Malformed_Post_Body_Returns_400()
    {
        // Arrange: deliberately malformed JSON (missing closing brace)
        var malformedJson = "{ \"date\": \"2024-01-01\", \"temperatureC\": 20, \"summary\": \"test\" ";

        var content = new StringContent(
            malformedJson,
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync(
            "/weatherforecast",
            content);

        // Assert HTTP status
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Assert DB was NOT modified
        await using var context = new ApplicationDbContext(_fixture.Options);
        var count = await context.WeatherForecasts.CountAsync();

        Assert.Equal(0, count);
    } 
    
    [Fact]
    public async Task Post_With_Missing_Date_Returns_400()
    {
        // Arrange: valid JSON but missing "date"
        var invalidJson = """
                          {
                              "temperatureC": 25,
                              "summary": "test"
                          }
                          """;

        var content = new StringContent(
            invalidJson,
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync(
            "/weatherforecast",
            content);

        // Assert HTTP status
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Assert DB unchanged
        await using var context = new ApplicationDbContext(_fixture.Options);
        var count = await context.WeatherForecasts.CountAsync();

        Assert.Equal(0, count);
    }
    [Fact]
    public async Task Post_With_Invalid_Data_Types_Returns_400()
    {
        // Arrange — capture initial state
        await using var beforeContext = new ApplicationDbContext(_fixture.Options);
        var beforeCount = await beforeContext.WeatherForecasts.CountAsync();

        var invalidJson = """
                          {
                              "date": "not-a-date",
                              "temperatureC": "abc",
                              "summary": "test"
                          }
                          """;

        var content = new StringContent(
            invalidJson,
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync(
            "/weatherforecast",
            content);

        // Assert status
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Assert DB unchanged
        await using var afterContext = new ApplicationDbContext(_fixture.Options);
        var afterCount = await afterContext.WeatherForecasts.CountAsync();

        Assert.Equal(beforeCount, afterCount);
    }
    
    [Fact]
    public async Task Successful_Post_Should_Return_201_And_Location_Id_Should_Exist_In_DB()
    {
        // Arrange
        var newWeatherDto = new WeatherForecastDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 25,
            Summary = "clear"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/weatherforecast",
            newWeatherDto);

        // Assert 201
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Extract ID from Location header
        var location = response.Headers.Location;
        Assert.NotNull(location);

        var id = int.Parse(location.Segments.Last());

        // Verify DB contains the inserted record
        await using var context = new ApplicationDbContext(_fixture.Options);
        var inserted = await context.WeatherForecasts.FindAsync(id);

        Assert.NotNull(inserted);
        Assert.Equal(newWeatherDto.Date, inserted.Date);
        Assert.Equal(newWeatherDto.TemperatureC, inserted.TemperatureC);
        Assert.Equal(newWeatherDto.Summary, inserted.Summary);
    }
    [Fact]
    public async Task Post_Should_Return_Location_That_Points_To_Created_Resource()
    {
        // Arrange
        var newWeatherDto = new WeatherForecastDto
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 30,
            Summary = "sunny"
        };

        // Act - create resource
        var postResponse = await _client.PostAsJsonAsync(
            "/weatherforecast",
            newWeatherDto);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        // Extract ID from Location header
        var location = postResponse.Headers.Location!;
        var id = int.Parse(location.Segments.Last());

        // Act - follow Location
        var getResponse = await _client.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var returnedDto = await getResponse.Content
            .ReadFromJsonAsync<WeatherForecastDto>();
        
        // We just made this resource, so getting it should exist again
        Assert.NotNull(returnedDto);

        // Assert the resource matches what we created
        Assert.Equal(newWeatherDto.Date, returnedDto.Date);
        Assert.Equal(newWeatherDto.TemperatureC, returnedDto.TemperatureC);
        Assert.Equal(newWeatherDto.Summary, returnedDto.Summary);
    } 
}