namespace HackerNews.BestStories.Api.Common.Caching;

public static class CacheKeys
{
    public const string BestStoryIds = "hacker-news:best-story-ids";

    public static string StoryDetails(int id) => $"hacker-news:story:{id}";
}
