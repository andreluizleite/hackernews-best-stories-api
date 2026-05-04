# HackerNews Best Stories API

## Overview

This project is a RESTful API built with ASP.NET Core that retrieves the top **N best stories** from the Hacker News API, ordered by score in descending order.

The API is designed with performance, scalability, and external API protection in mind.

---

## Features

* Retrieve top **N best stories**
* Sorted by **score (descending)**
* Efficient handling of external API calls
* In-memory caching to reduce load on Hacker News API
* Concurrent data fetching with controlled parallelism
* Clean and testable architecture

---

## External API

This project integrates with the official Hacker News API:

* Best story IDs:
  https://hacker-news.firebaseio.com/v0/beststories.json

* Story details:
  https://hacker-news.firebaseio.com/v0/item/{id}.json

---

## Endpoint

### GET /api/stories/best?n={number}

#### Example

```
GET /api/stories/best?n=10
```

#### Response

```json
[
  {
    "title": "Example story",
    "uri": "https://example.com",
    "postedBy": "author",
    "time": "2024-01-01T12:00:00Z",
    "score": 1234,
    "commentCount": 100
  }
]
```

---

## Architecture

The application follows a clean and modular structure:

```
Controller
  → Service Layer
      → HackerNews Client (HttpClient)
      → Cache Layer (IMemoryCache)
```

### Key Components

* **Controller**
  Handles HTTP requests and validation

* **Service Layer**
  Business logic, sorting, filtering, orchestration

* **HackerNews Client**
  Responsible for external API communication

* **Caching**
  Reduces redundant calls to Hacker News API

---

## Performance Considerations

To avoid overloading the Hacker News API:

* Results are cached using `IMemoryCache`
* Story details are fetched in parallel with controlled concurrency
* Repeated requests reuse cached data
* Optional limit on `n` to prevent excessive load

---

## Assumptions

* `n` must be greater than 0
* A maximum limit (e.g., 100) may be enforced
* Stories marked as `deleted` or `dead` are ignored
* Missing fields are handled gracefully

---

## Running the Application

### Prerequisites

* .NET 8 SDK (or later)

### Run

```bash
dotnet restore
dotnet build
dotnet run
```

The API will be available at:

```
https://localhost:7071
```

Swagger UI:

```
https://localhost:7071/swagger
```

---

## Testing

Unit tests can be executed with:

```bash
dotnet test
```

---

## Possible Improvements

Given more time, the following enhancements could be implemented:

* Distributed caching (e.g., Redis)
* Rate limiting and circuit breaker (Polly)
* Background refresh of cached stories
* Pagination support
* Observability (logging, metrics, tracing)
* Load testing and benchmarking

---

## Notes

This project was developed as part of a coding challenge and focuses on clarity, correctness, and performance under realistic conditions.
