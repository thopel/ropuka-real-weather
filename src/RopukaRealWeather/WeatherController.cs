using System;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using UnityEngine;

namespace RopukaRealWeather;

public sealed class WeatherController : MonoBehaviour
{
    private ModConfig _config = null!;
    private ManualLogSource _log = null!;
    private readonly GameWeatherAdapter _adapter = new GameWeatherAdapter();
    private OpenMeteoClient? _client;
    private CancellationTokenSource? _shutdown;
    private Task? _request;
    private readonly object _resultLock = new object();
    private WeatherObservation? _pending;
    private Exception? _pendingError;
    private float _nextRequestAt;
    private bool _warnedUnset;
    private bool _warnedDayNight;
    private bool _wasActive;
    private bool _startupFallbackApplied;
    private bool _isShutdown;
    private float _nextUpdateAt;

    public void Initialize(ModConfig config, ManualLogSource log)
    {
        _config = config;
        _log = log;
        _client = new OpenMeteoClient();
        _shutdown = new CancellationTokenSource();
        _nextRequestAt = config.SyncOnStartup.Value ? 0f : Time.realtimeSinceStartup + config.RefreshIntervalMinutes.Value * 60f;
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextUpdateAt) return;
        _nextUpdateAt = Time.unscaledTime + 0.25f;
        DrainResult();
        var active = CanSynchronize();
        if (!active)
        {
            if (_wasActive) _adapter.Restore();
            _wasActive = false;
            return;
        }

        if (!_adapter.IsReady) return;
        _adapter.Activate();
        _wasActive = true;

        if (!_startupFallbackApplied)
        {
            try
            {
                _adapter.Apply(GameWeather.Clear);
                _startupFallbackApplied = true;
                _log.LogInfo("No weather observation yet; applying clear weather until the first Open-Meteo response.");
            }
            catch (Exception ex)
            {
                _startupFallbackApplied = true;
                _log.LogError($"Could not apply the startup clear-weather fallback: {ex}");
            }
        }

        if (_config.UseRealDayNight.Value && !_warnedDayNight)
        {
            _warnedDayNight = true;
            _log.LogWarning("UseRealDayNight is enabled, but this game build exposes no controllable day/night system; the option is ignored.");
        }

        if ((_request == null || _request.IsCompleted) && Time.realtimeSinceStartup >= _nextRequestAt)
        {
            _nextRequestAt = Time.realtimeSinceStartup + _config.RefreshIntervalMinutes.Value * 60f;
            _request = FetchAsync(_shutdown!.Token);
        }
    }

    private bool CanSynchronize()
    {
        if (!_config.Enabled.Value) return false;
        var latitude = _config.Latitude.Value;
        var longitude = _config.Longitude.Value;
        if (!ModConfig.CoordinatesAreValid(latitude, longitude))
        {
            if (!_warnedUnset) _log.LogError("Coordinates are outside valid ranges: latitude [-90,90], longitude [-180,180]. Synchronization is disabled.");
            _warnedUnset = true;
            return false;
        }
        if (ModConfig.CoordinatesAreUnset(latitude, longitude))
        {
            if (!_warnedUnset) _log.LogWarning("Latitude and Longitude are both 0.0. Set your location in BepInEx/config/com.ropuka.realweather.cfg; no weather request will be sent.");
            _warnedUnset = true;
            return false;
        }
        _warnedUnset = false;
        return true;
    }

    public void RequestImmediateRefresh()
    {
        _nextRequestAt = 0f;
    }

    private async Task FetchAsync(CancellationToken token)
    {
        try
        {
            var result = await _client!.GetCurrentAsync(_config.Latitude.Value, _config.Longitude.Value, token).ConfigureAwait(false);
            lock (_resultLock) _pending = result;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            lock (_resultLock) _pendingError = ex;
        }
    }

    private void DrainResult()
    {
        WeatherObservation? observation;
        Exception? error;
        lock (_resultLock)
        {
            observation = _pending;
            error = _pendingError;
            _pending = null;
            _pendingError = null;
        }
        if (error != null) _log.LogWarning($"Open-Meteo synchronization failed; retrying at the next interval: {error.Message}");
        if (observation == null || !_wasActive) return;
        var mapped = WeatherMapper.FromWmoCode(observation.WeatherCode);
        try
        {
            var changed = _adapter.Apply(mapped);
            if (changed || _config.DebugLogging.Value)
                _log.LogInfo($"Weather synchronized: WMO {observation.WeatherCode} -> {mapped}, {observation.TemperatureCelsius:0.#} °C, is_day={observation.IsDay}, changed={changed}.");
        }
        catch (Exception ex)
        {
            _log.LogError($"The game rejected weather effect {mapped}; keeping the current effect: {ex}");
        }
    }

    public void Shutdown()
    {
        if (_isShutdown) return;
        _isShutdown = true;
        _shutdown?.Cancel();
        if (_wasActive) _adapter.Restore();
        _client?.Dispose();
        _shutdown?.Dispose();
        _wasActive = false;
    }

    private void OnApplicationQuit() => Shutdown();
}
