# Hacker News Best Stories API

[![CI](https://github.com/andreluizleite/hackernews-best-stories-api/actions/workflows/ci.yml/badge.svg)](https://github.com/andreluizleite/hackernews-best-stories-api/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Release](https://img.shields.io/github/v/release/andreluizleite/hackernews-best-stories-api)](https://github.com/andreluizleite/hackernews-best-stories-api/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A portfolio-sized ASP.NET Core API that retrieves and ranks the best stories from the official Hacker News API. The project focuses on reliable HTTP integration, controlled concurrency, caching, observability endpoints, and automated testing without introducing unnecessary infrastructure.

## Why this project exists

Fetching the top stories is not a single upstream request. Hacker News first returns story identifiers and then requires one request per story. This API coordinates those calls while protecting both the application and the external dependency.

## Highlights

- ASP.NET Core and .NET 10
- Typed `HttpClient` integration with the official Hacker News API
- Standard .NET HTTP resilience pipeline
- Controlled parallelism with `SemaphoreSlim`
- In-memory caching for story identifiers and details
- Filtering and deterministic score ordering
- RFC 9457-style Problem Details responses
- Per-client API rate limiting
- Liveness and dependency-readiness health checks
- Unit and in-process API integration tests
- Docker image and GitHub Actions CI

## Request flow

```text
Client
  -> StoriesController
     -> BestStoriesService
        -> IMemoryCache
        -> HackerNewsClient
           -> Hacker News API
```

The service first reads the list of candidate identifiers. Story details are then fetched concurrently with a configurable concurrency limit. Valid stories are ordered by score and the requested number of results is returned.

## API

### Get best stories

```http
GET /api/stories/best?n=10
```

`n` must be between `1` and the configured `MaxStoriesRequestLimit`.

Example response:

```json
[
  {
    "title": "Example story",
    "uri": "https://example.com",
    "postedBy": "author",
    "time": "2026-01-01T12:00:00+00:00",
    "score": 1234,
    "commentCount": 100
  }
]
```

### Health checks

```http
GET /health/live
GET /health/ready
```

The liveness endpoint confirms that the application process is running. The readiness endpoint also verifies access to the Hacker News API.

## Run locally

Prerequisites:

- .NET 10 SDK

```powershell
dotnet restore
dotnet run --project src/HackerNews.BestStories.Api
```

Use the HTTP address displayed by ASP.NET Core and open `/swagger`.

Example:

```powershell
Invoke-RestMethod "http://localhost:5186/api/stories/best?n=10"
```

The exact local port is defined by `Properties/launchSettings.json` and may differ when a port is already in use.

## Run with Docker

```powershell
docker build -t hackernews-best-stories-api .
docker run --rm -p 8080:8080 hackernews-best-stories-api
```

Then open `http://localhost:8080/swagger`.

## Tests

```powershell
dotnet test HackerNews.BestStories.sln --configuration Release
```

The test suite covers ranking, invalid upstream stories, mapping, caching, request validation, API serialization, and liveness.

## Configuration

Configuration is available under the `HackerNews` section in `appsettings.json`:

| Setting | Purpose |
| --- | --- |
| `BaseUrl` | Hacker News API base address |
| `MaxStoriesRequestLimit` | Maximum accepted value for `n` |
| `MaxConcurrentRequests` | Maximum simultaneous story-detail requests |
| `BestStoryIdsCacheMinutes` | Cache lifetime for the identifier list |
| `StoryDetailsCacheMinutes` | Cache lifetime for individual stories |

## Design decisions and trade-offs

- `IMemoryCache` keeps the sample easy to run. Multiple replicas would require a distributed cache if they needed to share cached state.
- Controlled concurrency reduces response time without creating an unbounded number of outbound requests.
- Partial item failures are logged and ignored, so one unavailable story does not fail the complete response.
- Readiness checks include the external API, while liveness intentionally does not.
- The project remains a focused API rather than pretending to be a microservices platform.

## Potential production evolution

- OpenTelemetry trace and metric export
- Distributed cache for multi-replica deployments
- Output caching with explicit freshness requirements
- Load tests and service-level objectives

These are documented as evolution options, not requirements for this portfolio scope.
