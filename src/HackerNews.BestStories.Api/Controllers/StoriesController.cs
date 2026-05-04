using HackerNews.BestStories.Api.Application.Interfaces;
using HackerNews.BestStories.Api.Common.Validation;
using HackerNews.BestStories.Api.Infrastructure.HackerNews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HackerNews.BestStories.Api.Controllers;

[ApiController]
[Route("api/stories")]
public sealed class StoriesController : ControllerBase
{
    private readonly IBestStoriesService _bestStoriesService;
    private readonly HackerNewsOptions _options;

    public StoriesController(
        IBestStoriesService bestStoriesService,
        IOptions<HackerNewsOptions> options)
    {
        _bestStoriesService = bestStoriesService;
        _options = options.Value;
    }

    [HttpGet("best")]
    public async Task<IActionResult> GetBestStories(
     [FromQuery] int n,
     CancellationToken cancellationToken)
    {
        if (!BestStoriesRequestValidator.IsValid(n, _options.MaxStoriesRequestLimit))
        {
            return Problem(
                title: "Invalid query parameter",
                detail: $"Parameter 'n' must be greater than 0 and less than or equal to {_options.MaxStoriesRequestLimit}.",
                statusCode: StatusCodes.Status400BadRequest,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                instance: HttpContext.Request.Path
            );
        }

        var stories = await _bestStoriesService.GetBestStoriesAsync(n, cancellationToken);

        return Ok(stories);
    }
}
