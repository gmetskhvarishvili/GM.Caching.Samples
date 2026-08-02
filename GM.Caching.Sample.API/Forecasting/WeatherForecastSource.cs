namespace GM.Caching.Sample.API.Forecasting;

/// <summary>A stand-in "expensive" source: it sleeps briefly and counts how often it is called.</summary>
public sealed class WeatherForecastSource : IForecastSource
{
    private static readonly string[] Summaries =
        ["Freezing", "Chilly", "Mild", "Warm", "Sweltering"];

    private int _calls;

    public int Calls => Volatile.Read(ref _calls);

    public async Task<Forecast> FetchAsync(string city, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _calls);
        await Task.Delay(200, cancellationToken); // simulate a slow upstream call

        var temp = Random.Shared.Next(-5, 35);
        var summary = Summaries[Random.Shared.Next(Summaries.Length)];
        return new Forecast(city, temp, summary, DateTimeOffset.UtcNow);
    }
}
