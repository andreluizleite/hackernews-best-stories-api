using HackerNews.BestStories.Api.Application.Interfaces;
using HackerNews.BestStories.Api.Application.Models;
using HackerNews.BestStories.Api.Common.Caching;
using HackerNews.BestStories.Api.Infrastructure.HackerNews;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HackerNews.BestStories.Api.Application.Services;

public sealed class BestStoriesService : IBestStoriesService
{
    private readonly IHackerNewsClient _hackerNewsClient;
    private readonly IMemoryCache _cache;
    private readonly HackerNewsOptions _options;
    private readonly ILogger<BestStoriesService> _logger;

    public BestStoriesService(
        IHackerNewsClient hackerNewsClient,
        IMemoryCache cache,
        IOptions<HackerNewsOptions> options,
        ILogger<BestStoriesService> logger)
    {
        _hackerNewsClient = hackerNewsClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<BestStoryResponse>> GetBestStoriesAsync(
        int n,
        CancellationToken cancellationToken)
    {
        var storyIds = await GetBestStoryIdsAsync(cancellationToken);

        var candidates = storyIds.ToList();

        var stories = await GetStoryDetailsAsync(candidates, cancellationToken);

        return stories
            .Where(IsValidStory)
            .OrderByDescending(story => story.Score)
            .Take(n)
            .Select(MapToResponse)
            .ToList();
    }

    private async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKeys.BestStoryIds, out IReadOnlyList<int>? cachedIds) &&
            cachedIds is not null)
        {
            return cachedIds;
        }

        var ids = await _hackerNewsClient.GetBestStoryIdsAsync(cancellationToken);

        _cache.Set(
            CacheKeys.BestStoryIds,
            ids,
            TimeSpan.FromMinutes(_options.BestStoryIdsCacheMinutes));

        return ids;
    }

    private async Task<IReadOnlyCollection<HackerNewsItemDto>> GetStoryDetailsAsync(
        IReadOnlyCollection<int> storyIds,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests);

        var tasks = storyIds.Select(async id =>
        {
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                return await GetStoryFromCacheOrApiAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process story {StoryId}", id);
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var result = await Task.WhenAll(tasks);

        return result
            .Where(story => story is not null)
            .Cast<HackerNewsItemDto>()
            .ToList();
    }

    private async Task<HackerNewsItemDto?> GetStoryFromCacheOrApiAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.StoryDetails(id);

        if (_cache.TryGetValue(cacheKey, out HackerNewsItemDto? cachedStory) &&
            cachedStory is not null)
        {
            return cachedStory;
        }

        var story = await _hackerNewsClient.GetItemByIdAsync(id, cancellationToken);

        if (story is not null)
        {
            _cache.Set(
                cacheKey,
                story,
                TimeSpan.FromMinutes(_options.StoryDetailsCacheMinutes));
        }

        return story;
    }

    private static bool IsValidStory(HackerNewsItemDto story)
    {
        return string.Equals(story.Type, "story", StringComparison.OrdinalIgnoreCase)
            && story.Deleted is not true
            && story.Dead is not true
            && !string.IsNullOrWhiteSpace(story.Title);
    }

    private static BestStoryResponse MapToResponse(HackerNewsItemDto story)
    {
        return new BestStoryResponse
        {
            Title = story.Title ?? string.Empty,
            Uri = story.Url ?? string.Empty,
            PostedBy = story.By ?? string.Empty,
            Time = DateTimeOffset.FromUnixTimeSeconds(story.Time),
            Score = story.Score,
            CommentCount = story.Descendants ?? 0
        };
    }
}
