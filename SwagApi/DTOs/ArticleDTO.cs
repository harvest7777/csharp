namespace SwagApi.DTOs;

public class PostArticleDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Slug { get; set; }
}
public class PutArticleDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Slug { get; set; }
}
public class ArticleDto
{
    public ArticleStatus Status { get; init; }

    public DateTime? PublishedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
    public bool IsDeleted { get; init; }

    public DateTime? DeletedAt { get; init; }
    public string? Content { get; init; }

    public string? Title { get; init; }

    public string? Slug { get; init; }
}
