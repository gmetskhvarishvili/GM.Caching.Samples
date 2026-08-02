using GM.Caching;

namespace GM.Caching.Sample.API.Forecasting;

/// <summary>
/// Wraps the expensive <see cref="IForecastSource"/> with <see cref="ICacheService"/>: repeated
/// requests for the same city are served from cache, and the source is called once per miss.
/// </summary>
public sealed class ForecastService(ICacheService cache, IForecastSource source)
{
    private static string Key(string city) => $"forecast:{city.ToLowerInvariant()}";

    public Task<Forecast> GetAsync(string city, CancellationToken cancellationToken = default) =>
        cache.GetOrCreateAsync(
            Key(city),
            ct => source.FetchAsync(city, ct),
            CacheEntryOptions.Absolute(TimeSpan.FromMinutes(10)),
            cancellationToken);

    public Task InvalidateAsync(string city, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(Key(city), cancellationToken);
}
