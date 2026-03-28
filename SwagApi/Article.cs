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
        if (title?.Length > 255)
            throw new ArgumentException("Title is too long.", nameof(title));

        Title = title;
        Content = content;
        Slug = slug;
        UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public void Publish()
    {
        if (string.IsNullOrEmpty(Title))
            throw new InvalidOperationException("Title is required.");

        if (string.IsNullOrEmpty(Content))
            throw new InvalidOperationException("Content is required.");

        Status = ArticleStatus.Published;
        PublishedAt = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
