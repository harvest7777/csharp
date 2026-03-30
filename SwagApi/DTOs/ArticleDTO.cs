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
    public ArticleStatus Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Content { get; set; }

    public string? Title { get; set; }

    public string? Slug { get; set; }
}
