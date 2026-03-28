namespace SwagApi.UnitTests.Articles;

public class ArticleTests
{
    [Fact]
    public void Update_ShouldThrow_WhenTooLongTitle()
    {
        // Arrange
        var article = new Article();
        var longTitle = new string('x', 256);

        // Assert
        Assert.Throws<ArgumentException>(() => article.Update(longTitle, null, null));
    }

    [Fact]
    public void Update_ShouldSet_WhenValid()
    {
        // Arrange
        var article = new Article();

        // Act
        article.Update("My Title", "Some content", "my-title");

        // Assert
        Assert.Equal("My Title", article.Title);
        Assert.Equal("Some content", article.Content);
        Assert.Equal("my-title", article.Slug);
    }

    [Fact]
    public void Publish_ShouldThrow_WhenMissingTitle()
    {
        // Arrange
        var article = new Article();
        article.Update(null, "Valid content", "valid-slug");


        // Assert
        Assert.Throws<InvalidOperationException>(() => article.Publish());
    }

    [Fact]
    public void Publish_ShouldThrow_WhenMissingContent()
    {
        // Arrange
        var article = new Article();
        article.Update("My Title", null, null);

        // Assert
        Assert.Throws<InvalidOperationException>(() => article.Publish());
    }

    [Fact]
    public void Publish_ShouldSetStatus_WhenValid()
    {
        // Arrange
        var article = new Article();
        article.Update("My Title", "Some content", "my-title");

        // Act
        article.Publish();

        // Assert
        Assert.Equal(ArticleStatus.Published, article.Status);
    }

    [Fact]
    public void Update_ShouldChange_UpdatedAt()
    {
        // Arrange
        var article = new Article();
        var before = article.UpdatedAt;

        // Act
        article.Update("My Title", "Some content", "my-title");

        // Assert
        Assert.NotEqual(before, article.UpdatedAt);
    }
}
