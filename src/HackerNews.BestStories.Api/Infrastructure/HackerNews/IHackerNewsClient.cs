namespace HackerNews.BestStories.Api.Infrastructure.HackerNews;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken);

    Task<HackerNewsItemDto?> GetItemByIdAsync(
        int id,
        CancellationToken cancellationToken);
}
