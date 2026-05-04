namespace HackerNews.BestStories.Api.Application.Models;

public sealed class BestStoryResponse
{
    public string Title { get; init; } = string.Empty;
    public string Uri { get; init; } = string.Empty;
    public string PostedBy { get; init; } = string.Empty;
    public DateTimeOffset Time { get; init; }
    public int Score { get; init; }
    public int CommentCount { get; init; }
}
