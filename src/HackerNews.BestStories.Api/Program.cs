using System.Text.Json;
using System.Threading.RateLimiting;
using HackerNews.BestStories.Api.Application.Interfaces;
using HackerNews.BestStories.Api.Application.Services;
using HackerNews.BestStories.Api.Infrastructure.HackerNews;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck<HackerNewsHealthCheck>("hacker-news-api", tags: ["ready"]);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection(HackerNewsOptions.SectionName));

var hackerNewsOptions = builder.Configuration
    .GetSection(HackerNewsOptions.SectionName)
    .Get<HackerNewsOptions>() ?? new HackerNewsOptions();

builder.Services.AddMemoryCache();

builder.Services
    .AddHttpClient<IHackerNewsClient, HackerNewsClient>(client =>
    {
        client.BaseAddress = new Uri(hackerNewsOptions.BaseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .AddStandardResilienceHandler();

builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRateLimiter();

app.MapControllers()
    .RequireRateLimiting("api");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString()
            })
        }));
    }
});

app.Run();

public partial class Program
{
}
