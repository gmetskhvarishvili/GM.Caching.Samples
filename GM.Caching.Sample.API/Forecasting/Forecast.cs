namespace GM.Caching.Sample.API.Forecasting;

/// <summary>A simple forecast payload returned (and cached) by the sample.</summary>
public sealed record Forecast(string City, int TemperatureC, string Summary, DateTimeOffset GeneratedAt);
