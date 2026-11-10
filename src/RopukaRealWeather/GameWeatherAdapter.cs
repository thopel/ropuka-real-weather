using UnityEngine;

namespace RopukaRealWeather;

public sealed class GameWeatherAdapter
{
    private WeatherManager? _manager;

    public bool IsReady => ResolveManager();

    public WeatherManager.WeatherType? Current => ResolveManager() ? _manager!.currentWeather : null;

    public bool Apply(GameWeather weather)
    {
        if (!ResolveManager()) return false;
        var target = ToGameWeather(weather);
        if (_manager!.currentWeather == target) return false;
        _manager.SetWeather(target);
        return true;
    }

    public void Activate() { }

    public void Restore()
    {
        // Do not alter the game's own settings or random-weather scheduler.
    }

    private bool ResolveManager()
    {
        if (_manager) return true;
        _manager = Object.FindFirstObjectByType<WeatherManager>();
        return _manager;
    }

    internal static WeatherManager.WeatherType ToGameWeather(GameWeather weather)
    {
        switch (weather)
        {
            case GameWeather.Clear: return WeatherManager.WeatherType.NatureB;
            case GameWeather.Cloudy: return WeatherManager.WeatherType.WindA;
            case GameWeather.Fog: return WeatherManager.WeatherType.WindA; // no fog effect in game
            case GameWeather.LightRain: return WeatherManager.WeatherType.RainB;
            case GameWeather.HeavyRain: return WeatherManager.WeatherType.RainA;
            case GameWeather.Snow: return WeatherManager.WeatherType.Snow;
            case GameWeather.Thunderstorm: return WeatherManager.WeatherType.RainA; // no lightning effect
            default: return WeatherManager.WeatherType.WindA;
        }
    }

}
