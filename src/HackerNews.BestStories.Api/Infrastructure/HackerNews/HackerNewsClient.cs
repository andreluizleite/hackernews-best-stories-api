using System.Net.Http.Json;

namespace HackerNews.BestStories.Api.Infrastructure.HackerNews;

public sealed class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HackerNewsClient> _logger;

    public HackerNewsClient(
        HttpClient httpClient,
        ILogger<HackerNewsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(
        CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetFromJsonAsync<List<int>>(
            "v0/beststories.json",
            cancellationToken);

        return result ?? [];
    }

    public async Task<HackerNewsItemDto?> GetItemByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<HackerNewsItemDto>(
                $"v0/item/{id}.json",
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve Hacker News item {StoryId}", id);
            return null;
        }
    }
}
