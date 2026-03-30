using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;
using SwagApi.DTOs;

namespace IntegrationTests;

public class ArticleIntegrationTests
    : IClassFixture<PostgresTestContainer>, IAsyncLifetime
{
    private readonly PostgresTestContainer _fixture;
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    public ArticleIntegrationTests(PostgresTestContainer fixture)
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
    public async Task Post_Should_InsertRecord_WhenValidDto()
    {
        // Arrange
        var newArticleDto = new PostArticleDto
        {
            Title = "New Article",
            Content = "New Article Content",
            Slug = "new-article",
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/article",
            newArticleDto);

        // This should have inserted the new article into the database.
        await using var context = new ApplicationDbContext(_fixture.Options);
        var count = await context.Articles.CountAsync();
        Assert.Equal(1, count);

        var id = int.Parse(response.Headers.Location!.Segments.Last());

        var inserted = await context.Articles
            .SingleAsync(w => w.Id == id);

        // The inserted weather forecast should be the same one that we made in the post request.
        Assert.Equal(newArticleDto.Title, inserted.Title);
        Assert.Equal(newArticleDto.Content, inserted.Content);
        Assert.Equal(newArticleDto.Slug, inserted.Slug);
    }

    [Fact]
    public async Task Put_Should_UpdateFields_WhenArticleExists()
    {

    }

    [Fact]
    public async Task Put_ShouldNot_NullifyFields_WhenFieldsOmitted()
    {

    }

    [Fact]
    public async Task Put_Should_ReturnNotFound_WhenArticleDoesNotExist()
    {

    }

    [Fact]
    public async Task Get_Should_ReturnArticle_WhenArticleExists()
    {}

    [Fact]
    public async Task Get_Should_ReturnNotFound_WhenArticleDoesNotExist()
    {}

    [Fact]
    public async Task Delete_Should_DeleteArticle_WhenArticleExists()
    {}

    [Fact]
    public async Task Delete_Should_ReturnNoContent_WhenArticleExists()
    {

    }

    [Fact]
    public async Task Delete_Should_ReturnNotFound_WhenArticleDoesNotExist()
    {

    }
}
