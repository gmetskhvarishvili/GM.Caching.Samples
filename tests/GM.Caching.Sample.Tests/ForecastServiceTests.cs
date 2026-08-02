using GM.Caching;
using GM.Caching.Sample.API.Forecasting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace GM.Caching.Sample.Tests;

public class ForecastServiceTests
{
    private static ForecastService NewService(out IForecastSource source)
    {
        var cache = new MemoryCacheService(
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new CacheServiceOptions()));
        source = new WeatherForecastSource();
        return new ForecastService(cache, source);
    }

    [Fact]
    public async Task RepeatedGets_HitTheSourceOnlyOnce()
    {
        var service = NewService(out var source);

        var first = await service.GetAsync("Tbilisi");
        var second = await service.GetAsync("Tbilisi");

        Assert.Equal(1, source.Calls);              // second request served from cache
        Assert.Equal(first, second);                // identical cached instance/value
    }

    [Fact]
    public async Task DifferentCities_AreCachedIndependently()
    {
        var service = NewService(out var source);

        await service.GetAsync("Tbilisi");
        await service.GetAsync("Batumi");
        await service.GetAsync("Tbilisi");

        Assert.Equal(2, source.Calls);              // one call per distinct city
    }

    [Fact]
    public async Task Invalidate_ForcesTheNextGetToRefetch()
    {
        var service = NewService(out var source);

        await service.GetAsync("Tbilisi");
        await service.InvalidateAsync("Tbilisi");
        await service.GetAsync("Tbilisi");

        Assert.Equal(2, source.Calls);
    }

    [Fact]
    public async Task ConcurrentGets_ForSameCity_HitTheSourceOnce()
    {
        var service = NewService(out var source);

        var results = await Task.WhenAll(
            Enumerable.Range(0, 20).Select(_ => service.GetAsync("Tbilisi")));

        Assert.Equal(1, source.Calls);              // single-flight: no stampede
        Assert.All(results, r => Assert.Equal(results[0], r));
    }
}
