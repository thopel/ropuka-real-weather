namespace RopukaRealWeather;

public enum GameWeather
{
    Clear,
    Cloudy,
    Fog,
    LightRain,
    HeavyRain,
    Snow,
    Thunderstorm
}

public static class WeatherMapper
{
    public static bool ShouldApply(GameWeather? current, GameWeather next) => !current.HasValue || current.Value != next;

    public static GameWeather FromWmoCode(int code)
    {
        if (code == 0 || code == 1) return GameWeather.Clear;
        if (code == 2 || code == 3) return GameWeather.Cloudy;
        if (code == 45 || code == 48) return GameWeather.Fog;
        if (code >= 51 && code <= 57) return GameWeather.LightRain;
        if (code >= 61 && code <= 67) return GameWeather.HeavyRain;
        if (code >= 71 && code <= 77) return GameWeather.Snow;
        if (code >= 80 && code <= 82) return GameWeather.HeavyRain;
        if (code >= 85 && code <= 86) return GameWeather.Snow;
        if (code >= 95 && code <= 99) return GameWeather.Thunderstorm;
        return GameWeather.Cloudy;
    }
}
