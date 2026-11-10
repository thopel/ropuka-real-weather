using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RopukaRealWeather;

public sealed class WeatherObservation
{
    public WeatherObservation(int weatherCode, bool isDay, double temperatureCelsius)
    {
        WeatherCode = weatherCode;
        IsDay = isDay;
        TemperatureCelsius = temperatureCelsius;
    }

    public int WeatherCode { get; }
    public bool IsDay { get; }
    public double TemperatureCelsius { get; }
}

public sealed class OpenMeteoClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsClient;

    public OpenMeteoClient(HttpClient? httpClient = null, TimeSpan? timeout = null)
    {
        _ownsClient = httpClient == null;
        _http = httpClient ?? new HttpClient();
        _http.Timeout = timeout ?? TimeSpan.FromSeconds(10);
    }

    public async Task<WeatherObservation> GetCurrentAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var url = "https://api.open-meteo.com/v1/forecast?latitude=" + latitude.ToString(CultureInfo.InvariantCulture) +
                  "&longitude=" + longitude.ToString(CultureInfo.InvariantCulture) +
                  "&current=weather_code,is_day,temperature_2m&timezone=auto";
        using var response = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        try
        {
            var current = JObject.Parse(json)["current"] ?? throw new FormatException("Open-Meteo response has no 'current' object.");
            var code = current.Value<int?>("weather_code") ?? throw new FormatException("Open-Meteo response has no weather_code.");
            var isDay = current.Value<int?>("is_day") ?? throw new FormatException("Open-Meteo response has no is_day.");
            var temperature = current.Value<double?>("temperature_2m") ?? throw new FormatException("Open-Meteo response has no temperature_2m.");
            return new WeatherObservation(code, isDay != 0, temperature);
        }
        catch (Exception ex) when (!(ex is FormatException))
        {
            throw new FormatException("Invalid Open-Meteo JSON response.", ex);
        }
    }

    public void Dispose()
    {
        if (_ownsClient) _http.Dispose();
    }
}
