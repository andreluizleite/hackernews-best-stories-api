using HackerNews.BestStories.Api.Application.Services;
using HackerNews.BestStories.Api.Infrastructure.HackerNews;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HackerNews.BestStories.Tests.Services;

public sealed class BestStoriesServiceTests
{
    [Fact]
    public async Task GetBestStoriesAsync_ShouldReturnTopNStoriesOrderedByScoreDescending()
    {
        // Arrange
        var client = new FakeHackerNewsClient(
            bestStoryIds: [1, 2, 3],
            items: new Dictionary<int, HackerNewsItemDto>
            {
                [1] = CreateStory(id: 1, score: 100, title: "Story 1"),
                [2] = CreateStory(id: 2, score: 300, title: "Story 2"),
                [3] = CreateStory(id: 3, score: 200, title: "Story 3")
            });

        var service = CreateService(client);

        // Act
        var result = await service.GetBestStoriesAsync(2, CancellationToken.None);

        // Assert
        var stories = result.ToList();

        Assert.Equal(2, stories.Count);
        Assert.Equal("Story 2", stories[0].Title);
        Assert.Equal(300, stories[0].Score);
        Assert.Equal("Story 3", stories[1].Title);
        Assert.Equal(200, stories[1].Score);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldIgnoreDeletedDeadAndInvalidStories()
    {
        // Arrange
        var client = new FakeHackerNewsClient(
            bestStoryIds: [1, 2, 3, 4],
            items: new Dictionary<int, HackerNewsItemDto>
            {
                [1] = CreateStory(id: 1, score: 100, title: "Valid Story"),
                [2] = CreateStory(id: 2, score: 999, title: "Deleted Story", deleted: true),
                [3] = CreateStory(id: 3, score: 888, title: "Dead Story", dead: true),
                [4] = CreateStory(id: 4, score: 777, title: "")
            });

        var service = CreateService(client);

        // Act
        var result = await service.GetBestStoriesAsync(10, CancellationToken.None);

        // Assert
        var stories = result.ToList();

        Assert.Single(stories);
        Assert.Equal("Valid Story", stories[0].Title);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldMapHackerNewsItemToResponseCorrectly()
    {
        // Arrange
        var unixTime = 1570887781;

        var client = new FakeHackerNewsClient(
            bestStoryIds: [1],
            items: new Dictionary<int, HackerNewsItemDto>
            {
                [1] = new()
                {
                    Id = 1,
                    Type = "story",
                    Title = "A uBlock Origin update was rejected from the Chrome Web Store",
                    Url = "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
                    By = "ismaildonmez",
                    Time = unixTime,
                    Score = 1716,
                    Descendants = 572
                }
            });

        var service = CreateService(client);

        // Act
        var result = await service.GetBestStoriesAsync(1, CancellationToken.None);

        // Assert
        var story = Assert.Single(result);

        Assert.Equal("A uBlock Origin update was rejected from the Chrome Web Store", story.Title);
        Assert.Equal("https://github.com/uBlockOrigin/uBlock-issues/issues/745", story.Uri);
        Assert.Equal("ismaildonmez", story.PostedBy);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(unixTime), story.Time);
        Assert.Equal(1716, story.Score);
        Assert.Equal(572, story.CommentCount);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ShouldUseCachedStoryDetailsOnRepeatedCalls()
    {
        // Arrange
        var client = new FakeHackerNewsClient(
            bestStoryIds: [1],
            items: new Dictionary<int, HackerNewsItemDto>
            {
                [1] = CreateStory(id: 1, score: 100, title: "Cached Story")
            });

        var service = CreateService(client);

        // Act
        await service.GetBestStoriesAsync(1, CancellationToken.None);
        await service.GetBestStoriesAsync(1, CancellationToken.None);

        // Assert
        Assert.Equal(1, client.GetBestStoryIdsCallCount);
        Assert.Equal(1, client.GetItemByIdCallCount);
    }

    private static BestStoriesService CreateService(FakeHackerNewsClient client)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());

        var options = Options.Create(new HackerNewsOptions
        {
            MaxStoriesRequestLimit = 100,
            MaxConcurrentRequests = 10,
            BestStoryIdsCacheMinutes = 5,
            StoryDetailsCacheMinutes = 30
        });

        return new BestStoriesService(
            client,
            cache,
            options,
            NullLogger<BestStoriesService>.Instance);
    }

    private static HackerNewsItemDto CreateStory(
        int id,
        int score,
        string title,
        bool deleted = false,
        bool dead = false)
    {
        return new HackerNewsItemDto
        {
            Id = id,
            Type = "story",
            Title = title,
            Url = $"https://example.com/story-{id}",
            By = "test-user",
            Time = 1570887781,
            Score = score,
            Descendants = 10,
            Deleted = deleted,
            Dead = dead
        };
    }

    private sealed class FakeHackerNewsClient : IHackerNewsClient
    {
        private readonly IReadOnlyList<int> _bestStoryIds;
        private readonly IReadOnlyDictionary<int, HackerNewsItemDto> _items;

        public int GetBestStoryIdsCallCount { get; private set; }

        public int GetItemByIdCallCount { get; private set; }

        public FakeHackerNewsClient(
            IReadOnlyList<int> bestStoryIds,
            IReadOnlyDictionary<int, HackerNewsItemDto> items)
        {
            _bestStoryIds = bestStoryIds;
            _items = items;
        }

        public Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
        {
            GetBestStoryIdsCallCount++;
            return Task.FromResult(_bestStoryIds);
        }

        public Task<HackerNewsItemDto?> GetItemByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            GetItemByIdCallCount++;

            _items.TryGetValue(id, out var item);

            return Task.FromResult(item);
        }
    }
}