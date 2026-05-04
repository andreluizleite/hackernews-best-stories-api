using HackerNews.BestStories.Api.Application.Interfaces;
using HackerNews.BestStories.Api.Application.Services;
using HackerNews.BestStories.Api.Infrastructure.HackerNews;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection(HackerNewsOptions.SectionName));

var hackerNewsOptions = builder.Configuration
    .GetSection(HackerNewsOptions.SectionName)
    .Get<HackerNewsOptions>() ?? new HackerNewsOptions();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>(client =>
{
    client.BaseAddress = new Uri(hackerNewsOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.Run();

public partial class Program
{
}
