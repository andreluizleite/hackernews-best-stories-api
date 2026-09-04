using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HackerNews.BestStories.Api.Infrastructure.HackerNews;

public sealed class HackerNewsHealthCheck : IHealthCheck
{
    private readonly IHackerNewsClient _client;

    public HackerNewsHealthCheck(IHackerNewsClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var storyIds = await _client.GetBestStoryIdsAsync(cancellationToken);

            return storyIds.Count > 0
                ? HealthCheckResult.Healthy("Hacker News API is responding.")
                : HealthCheckResult.Degraded("Hacker News API returned no best stories.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Hacker News API is unavailable.",
                exception);
        }
    }
}
