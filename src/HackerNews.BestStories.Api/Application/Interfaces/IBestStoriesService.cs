using HackerNews.BestStories.Api.Application.Models;

namespace HackerNews.BestStories.Api.Application.Interfaces;

public interface IBestStoriesService
{
    Task<IReadOnlyCollection<BestStoryResponse>> GetBestStoriesAsync(
        int n,
        CancellationToken cancellationToken);
}
