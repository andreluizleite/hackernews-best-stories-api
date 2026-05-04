namespace HackerNews.BestStories.Api.Common.Validation;

public static class BestStoriesRequestValidator
{
    public static bool IsValid(int n, int maxLimit)
    {
        return n > 0 && n <= maxLimit;
    }
}
