using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwagApi.DTOs;
using SwagApi.Data;

namespace SwagApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WeatherForecastController(ApplicationDbContext context)
    {
        _context = context;
    } 

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult<WeatherForecastDto[]>> Get()
    {
        var res = await _context.WeatherForecasts.ToArrayAsync();
        return Ok(res);
    }
}