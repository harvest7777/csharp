namespace SwagApi;

public enum ArticleStatus
{
    Draft,
    Published
}

public class Article
{
    public int Id { get; set; }   // Primary Key

    public ArticleStatus Status { get; set; } =  ArticleStatus.Draft;

    public DateOnly? PublishedAt { get; set; }

    public DateOnly UpdatedAt { get; set; }

    public string? Content { get; set; }

    public string? Title { get; set; }

    public string? Slug { get; set; }

}