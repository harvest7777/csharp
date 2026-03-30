using Microsoft.EntityFrameworkCore;

namespace SwagApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
    public DbSet<Article> Articles { get; set; }
}