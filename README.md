<p align="center">
  <img src="icon.png" alt="GM.Caching Samples" width="140" height="140" />
</p>

# GM.Caching Samples

[![CI](https://github.com/gmetskhvarishvili/GM.Caching.Samples/actions/workflows/ci.yml/badge.svg)](https://github.com/gmetskhvarishvili/GM.Caching.Samples/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A minimal ASP.NET Core Web API that shows **[GM.Caching](https://www.nuget.org/packages/GM.Caching)**
in action: caching an expensive call behind `ICacheService.GetOrCreateAsync`, and swapping between
the in-memory and **[GM.Caching.Redis](https://www.nuget.org/packages/GM.Caching.Redis)** backends
with one config setting. Targets **.NET 10**.

## What it demonstrates

- A `ForecastService` that wraps a deliberately slow `IForecastSource` with `ICacheService`:
  repeated requests for the same city are served from cache and the source is called **once per
  miss** (a call counter makes this visible).
- **Single-flight** `GetOrCreateAsync`: 20 concurrent requests for the same key hit the source once.
- Backend switching with **no code change** — `Cache:Provider` = `Memory` (default) or `Redis`.

## Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/forecast/{city}` | Returns the (cached) forecast plus `sourceCalls`, the number of real upstream calls so far |
| `DELETE` | `/api/v1/forecast/{city}` | Evicts the city's cache entry so the next GET refetches |
| `GET` | `/health/live` | Liveness probe — no downstream checks |
| `GET` | `/health/ready` | Readiness probe — runs registered health checks |

Call `GET /api/v1/forecast/Tbilisi` twice: the first response is slow and `sourceCalls` is `1`; the
second is instant and `sourceCalls` stays `1`.

## Running

```bash
dotnet run --project GM.Caching.Sample.API
```

To use Redis instead of in-memory, set `Cache:Provider` to `Redis` (and `ConnectionStrings:Redis`),
e.g. run a local Redis with `docker run -p 6379:6379 -d redis` and:

```bash
Cache__Provider=Redis dotnet run --project GM.Caching.Sample.API
```

## Testing

```bash
dotnet test
```

The tests drive `ForecastService` against the in-memory cache and assert the source is hit only on
misses (including under concurrency) — no Redis required.

## License

MIT — see [LICENSE](LICENSE).
