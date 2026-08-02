namespace GM.Caching.Sample.API.Forecasting;

/// <summary>
/// The "expensive" upstream a real app would call (a weather API, a database, …). The sample uses
/// it to show that caching means it is hit only on a miss.
/// </summary>
public interface IForecastSource
{
    /// <summary>Number of times <see cref="FetchAsync"/> actually ran — handy for demos and tests.</summary>
    int Calls { get; }

    Task<Forecast> FetchAsync(string city, CancellationToken cancellationToken = default);
}
