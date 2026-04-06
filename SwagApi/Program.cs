using System.Threading.RateLimiting;
using Auth0.AspNetCore.Authentication.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using SwagApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("API");

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string"
                                           + "'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuth0ApiAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.JwtBearerOptions = new JwtBearerOptions
    {
        Audience = builder.Configuration["Auth0:Audience"],
        Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                logger.LogInformation("Token Validated {Token}...",  context.SecurityToken.ToString().Substring(0,10));
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                logger.LogInformation("Authentication Failed {Error}...",  context.Exception);
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                logger.LogInformation("Forbidden {Reason}...",  context.Result.Failure);
                return Task.CompletedTask;
            }
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRequestAuth();

app.UseRateLimiter();

app.MapControllers();

app.Run();