using GM.Caching;
using GM.Caching.Redis;
using GM.Caching.Sample.API.Forecasting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Choose the cache backend from configuration: "Cache:Provider" = "Redis" or "Memory" (default).
// Both expose the same ICacheService, so nothing else in the app changes.
if (string.Equals(builder.Configuration["Cache:Provider"], "Redis", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddGMRedisCaching(o =>
    {
        o.ConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        o.KeyPrefix = "gm-caching-sample:";
    });
}
else
{
    builder.Services.AddGMCaching(o => o.KeyPrefix = "gm-caching-sample:");
}

builder.Services.AddSingleton<IForecastSource, WeatherForecastSource>();
builder.Services.AddScoped<ForecastService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Liveness must not depend on downstream dependencies, so it runs no checks; readiness runs
// every registered health check (none here yet). See engineering baseline §11.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

// First call for a city is slow (cache miss -> source); subsequent calls are fast (cache hit),
// and 'sourceCalls' stops increasing — that's GetOrCreateAsync at work.
app.MapGet("/api/v1/forecast/{city}", async (string city, ForecastService forecasts, IForecastSource source) =>
{
    var forecast = await forecasts.GetAsync(city);
    return Results.Ok(new { forecast, sourceCalls = source.Calls });
});

app.MapDelete("/api/v1/forecast/{city}", async (string city, ForecastService forecasts) =>
{
    await forecasts.InvalidateAsync(city);
    return Results.NoContent();
});

await app.RunAsync();
