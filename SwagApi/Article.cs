namespace SwagApi;

public enum ArticleStatus
{
    Draft,
    Published
}

public class Article
{
    public int Id { get; set; }   // Primary Key

    public ArticleStatus Status { get; private set; } =  ArticleStatus.Draft;

    public DateOnly? PublishedAt { get; private set; }

    public DateOnly UpdatedAt { get; private set; }

    public string? Content { get; private set; }

    public string? Title { get; private set; }

    public string? Slug { get; private set; }

    public void Update(string? title, string? content, string? slug)
    {
        throw new NotImplementedException();
    }

    public void Publish()
    {
        throw new NotImplementedException();
    }
}
