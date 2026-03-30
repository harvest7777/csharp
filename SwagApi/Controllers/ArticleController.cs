using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;
using SwagApi.DTOs;

namespace SwagApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticleController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ArticleController(ApplicationDbContext context)
    {
        _context = context;
    } 

    [HttpGet(Name = "GetArticles")]
    public async Task<ActionResult<ArticleDto[]>> Get([FromQuery] string status = "active")
    {
        var query = status switch
        {
            "deleted" => _context.Articles.IgnoreQueryFilters().Where(a => a.IsDeleted),
            "all"     => _context.Articles.IgnoreQueryFilters(),
            // This also handles nonsense values like "foo" or lack of query param
            _         => _context.Articles.Where(a => !a.IsDeleted)
        };
        return Ok(await query.ToArrayAsync());
    }

    [HttpGet("{id}", Name = "GetArticle")]
    public async Task<ActionResult<ArticleDto>> GetById(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article is null) return NotFound();
        return Ok(article);
    }

    [HttpPost(Name = "PostArticle")]
    public async Task<ActionResult<ArticleDto>> Post([FromBody] PostArticleDto dto)
    {
        var newArticle = new Article();
        newArticle.Update(dto.Title, dto.Content, dto.Slug);
        _context.Add(newArticle);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = newArticle.Id }, newArticle);
    }


    [HttpDelete("{id}", Name = "DeleteArticle")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article is null) return NotFound();
        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}", Name = "PutArticle")]

    public async Task<ActionResult<ArticleDto>> Put(int id, [FromBody] PutArticleDto dto)
    {
        // EF will get a tracked version of the article object which has a mapping to the database
        var article = await _context.Articles.FindAsync(id);

        if (article == null)
            return NotFound();

        // Literally stages updates into the database just from using C# methods!
        article.Update(dto.Title, dto.Content, dto.Slug);

        // Blocking write
        await _context.SaveChangesAsync();
        return NoContent();
    }
}