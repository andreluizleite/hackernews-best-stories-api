using System.Text.Json.Serialization;

namespace HackerNews.BestStories.Api.Infrastructure.HackerNews;

public sealed class HackerNewsItemDto
{
    public int Id { get; set; }

    public string? Type { get; set; }

    [JsonPropertyName("by")]
    public string? By { get; set; }

    public long Time { get; set; }

    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    public int Score { get; set; }

    [JsonPropertyName("descendants")]
    public int? Descendants { get; set; }

    public bool? Deleted { get; set; }

    public bool? Dead { get; set; }
}
