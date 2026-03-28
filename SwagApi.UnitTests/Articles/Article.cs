namespace SwagApi.UnitTests.Articles;

public class UnitTest1
{
    [Fact]
    public void Publish_ShouldThrow_WhenMissingTitle()
    {
        var article = new Article();
        Assert.Throws<InvalidOperationException>(() => article.Publish());
    }

    [Fact]
    public void Publish_ShouldThrow_WhenMissingContent()
    {

    }

    [Fact]
    public void Publish_ShouldSetStatus_WhenValid()
    {
    }
}