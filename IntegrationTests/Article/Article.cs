using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SwagApi;
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

    private async Task<int> CreateArticleAsync(PostArticleDto dto)
    {
        var response = await _client.PostAsJsonAsync("/article", dto);
        return int.Parse(response.Headers.Location!.Segments.Last());
    }

    [Fact]
    public async Task Post_Should_InsertRecord_WhenValid()
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
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(1, count);

        var id = int.Parse(response.Headers.Location!.Segments.Last());

        var inserted = await context.Articles
            .SingleAsync(w => w.Id == id);

        // The inserted article should be the same one that we made in the post request.
        Assert.Equal(newArticleDto.Title, inserted.Title);
        Assert.Equal(newArticleDto.Content, inserted.Content);
        Assert.Equal(newArticleDto.Slug, inserted.Slug);
    }

    [Fact]
    public async Task Put_Should_UpdateFields_WhenArticleExists()
    {
        // Arrange
        var id = await CreateArticleAsync(new PostArticleDto
        {
            Title = "Original Title",
            Content = "Original Content",
            Slug = "original-slug"
        });

        var updateDto = new PutArticleDto
        {
            Title = "Updated Title",
            Content = "Updated Content",
            Slug = "updated-slug"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/article/{id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var context = new ApplicationDbContext(_fixture.Options);
        var article = await context.Articles.SingleAsync(a => a.Id == id);
        Assert.Equal(updateDto.Title, article.Title);
        Assert.Equal(updateDto.Content, article.Content);
        Assert.Equal(updateDto.Slug, article.Slug);
    }

    [Fact]
    public async Task Put_ShouldNot_NullifyFields_WhenFieldsOmitted()
    {
        // Arrange
        var id = await CreateArticleAsync(new PostArticleDto
        {
            Title = "Original Title",
            Content = "Original Content",
            Slug = "original-slug"
        });

        // Act — only send title, omit content and slug
        var response = await _client.PutAsJsonAsync($"/article/{id}", new PutArticleDto
        {
            Title = "Updated Title"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var context = new ApplicationDbContext(_fixture.Options);
        var article = await context.Articles.SingleAsync(a => a.Id == id);
        Assert.Equal("Original Content", article.Content);
        Assert.Equal("original-slug", article.Slug);
    }

    [Fact]
    public async Task Put_Should_ReturnNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        var dto = new PutArticleDto { Title = "Title", Content = "Content", Slug = "slug" };

        // Act
        var response = await _client.PutAsJsonAsync("/article/999", dto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Should_ReturnArticle_WhenArticleExists()
    {
        // Arrange
        var id = await CreateArticleAsync(new PostArticleDto
        {
            Title = "Test Article",
            Content = "Test Content",
            Slug = "test-article"
        });

        // Act
        var response = await _client.GetAsync($"/article/{id}");
        var article = await response.Content.ReadFromJsonAsync<ArticleDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Test Article", article!.Title);
        Assert.Equal("Test Content", article.Content);
        Assert.Equal("test-article", article.Slug);
    }

    [Fact]
    public async Task Get_Should_ReturnNotFound_WhenArticleDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/article/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Should_ReturnOnlyActiveArticles_WhenStatusIsActive()
    {
        // Arrange — seed one active and one deleted
        await using var context = new ApplicationDbContext(_fixture.Options);
        var active = new Article();
        active.Update("Active Article", "Content", "active");
        var deleted = new Article();
        deleted.Update("Deleted Article", "Content", "deleted");
        deleted.Delete();
        context.Articles.AddRange(active, deleted);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/article?status=active");
        var articles = await response.Content.ReadFromJsonAsync<ArticleDto[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, articles!.Length);
        Assert.Equal("Active Article", articles[0].Title);
    }

    [Fact]
    public async Task Get_Should_ReturnOnlyActiveArticles_WhenNoStatusParameter()
    {
        // Arrange — seed one active and one deleted
        await using var context = new ApplicationDbContext(_fixture.Options);
        var active = new Article();
        active.Update("Active Article", "Content", "active");
        var deleted = new Article();
        deleted.Update("Deleted Article", "Content", "deleted");
        deleted.Delete();
        context.Articles.AddRange(active, deleted);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/article");
        var articles = await response.Content.ReadFromJsonAsync<ArticleDto[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, articles!.Length);
        Assert.Equal("Active Article", articles[0].Title);
    }

    [Fact]
    public async Task Get_Should_ReturnOnlyActiveArticles_WhenNonsenseStatusParameter()
    {
        // Arrange — seed one active and one deleted
        await using var context = new ApplicationDbContext(_fixture.Options);
        var active = new Article();
        active.Update("Active Article", "Content", "active");
        var deleted = new Article();
        deleted.Update("Deleted Article", "Content", "deleted");
        deleted.Delete();
        context.Articles.AddRange(active, deleted);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/article?status=foo");
        var articles = await response.Content.ReadFromJsonAsync<ArticleDto[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, articles!.Length);
        Assert.All(articles, a => Assert.Equal("Active Article", a.Title));

    }

    [Fact]
    public async Task Get_Should_ReturnAllArticles_WhenStatusIsAll()
    {
        // Arrange — seed one active and one deleted
        await using var context = new ApplicationDbContext(_fixture.Options);
        var active = new Article();
        active.Update("Active Article", "Content", "active");
        var deleted = new Article();
        deleted.Update("Deleted Article", "Content", "deleted");
        deleted.Delete();
        context.Articles.AddRange(active, deleted);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/article?status=all");
        var articles = await response.Content.ReadFromJsonAsync<ArticleDto[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, articles!.Length);
    }

    [Fact]
    public async Task Get_Should_ReturnDeletedArticles_WhenStatusIsDeleted()
    {
        // Arrange — seed one active and one deleted
        await using var context = new ApplicationDbContext(_fixture.Options);
        var active = new Article();
        active.Update("Active Article", "Content", "active");
        var deleted = new Article();
        deleted.Update("Deleted Article", "Content", "deleted");
        deleted.Delete();
        context.Articles.AddRange(active, deleted);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/article?status=deleted");
        var articles = await response.Content.ReadFromJsonAsync<ArticleDto[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, articles!.Length);
        Assert.Equal("Deleted Article", articles[0].Title);
    }

    [Fact]
    public async Task Delete_Should_DeleteArticle_WhenArticleExists()
    {
        // Arrange
        var id = await CreateArticleAsync(new PostArticleDto
        {
            Title = "To Delete",
            Content = "Content",
            Slug = "to-delete"
        });

        // Act
        var response = await _client.DeleteAsync($"/article/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var context = new ApplicationDbContext(_fixture.Options);
        var exists = await context.Articles.AnyAsync(a => a.Id == id);
        Assert.False(exists);
    }

    [Fact]
    public async Task Delete_Should_ReturnNoContent_WhenArticleExists()
    {
        // Arrange
        var id = await CreateArticleAsync(new PostArticleDto
        {
            Title = "To Delete",
            Content = "Content",
            Slug = "to-delete"
        });

        // Act
        var response = await _client.DeleteAsync($"/article/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_ReturnNotFound_WhenArticleDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/article/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
