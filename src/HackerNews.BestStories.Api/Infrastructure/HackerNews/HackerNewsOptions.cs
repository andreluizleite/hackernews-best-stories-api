namespace HackerNews.BestStories.Api.Infrastructure.HackerNews;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    public string BaseUrl { get; init; } = "https://hacker-news.firebaseio.com/";

    public int MaxStoriesRequestLimit { get; init; } = 100;

    public int MaxConcurrentRequests { get; init; } = 10;

    public int BestStoryIdsCacheMinutes { get; init; } = 5;

    public int StoryDetailsCacheMinutes { get; init; } = 30;
}
