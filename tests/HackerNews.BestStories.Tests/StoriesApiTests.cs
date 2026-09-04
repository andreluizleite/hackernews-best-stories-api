using System.Net;
using System.Net.Http.Json;
using HackerNews.BestStories.Api.Application.Models;
using HackerNews.BestStories.Api.Infrastructure.HackerNews;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace HackerNews.BestStories.Tests;

public sealed class StoriesApiTests : IClassFixture<StoriesApiTests.ApiFactory>
{
    private readonly HttpClient _client;

    public StoriesApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetBestStories_ShouldReturnRankedStories()
    {
        var response = await _client.GetAsync("/api/stories/best?n=2");

        response.EnsureSuccessStatusCode();

        var stories = await response.Content
            .ReadFromJsonAsync<List<BestStoryResponse>>();

        Assert.NotNull(stories);
        Assert.Equal(2, stories.Count);
        Assert.Equal(300, stories[0].Score);
        Assert.Equal(200, stories[1].Score);
    }

    [Fact]
    public async Task GetBestStories_WithInvalidLimit_ShouldReturnProblemDetails()
    {
        var response = await _client.GetAsync("/api/stories/best?n=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task LiveHealthCheck_ShouldReturnHealthy()
    {
        var response = await _client.GetAsync("/health/live");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ReadyHealthCheck_ShouldVerifyTheExternalApi()
    {
        var response = await _client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("hacker-news-api", content);
        Assert.Contains("Healthy", content);
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IHackerNewsClient>();
                services.AddSingleton<IHackerNewsClient>(new FakeHackerNewsClient());
            });
        }
    }

    private sealed class FakeHackerNewsClient : IHackerNewsClient
    {
        private static readonly IReadOnlyDictionary<int, HackerNewsItemDto> Stories =
            new Dictionary<int, HackerNewsItemDto>
            {
                [1] = CreateStory(1, 100),
                [2] = CreateStory(2, 300),
                [3] = CreateStory(3, 200)
            };

        public Task<IReadOnlyList<int>> GetBestStoryIdsAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<int>>([1, 2, 3]);
        }

        public Task<HackerNewsItemDto?> GetItemByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            Stories.TryGetValue(id, out var story);
            return Task.FromResult(story);
        }

        private static HackerNewsItemDto CreateStory(int id, int score)
        {
            return new HackerNewsItemDto
            {
                Id = id,
                Type = "story",
                Title = $"Story {id}",
                Url = $"https://example.com/stories/{id}",
                By = "portfolio-test",
                Time = 1_570_887_781,
                Score = score,
                Descendants = 10
            };
        }
    }
}
