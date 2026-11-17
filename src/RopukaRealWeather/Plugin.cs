using BepInEx;

namespace RopukaRealWeather;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.ropuka.realweather";
    public const string PluginName = "Ropuka Real Weather";
    public const string PluginVersion = "2.0.3";

    private WeatherController? _controller;

    private void Awake()
    {
        var config = ModConfig.Bind(Config);
        _controller = gameObject.AddComponent<WeatherController>();
        _controller.Initialize(config, Logger);

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        _controller?.Shutdown();
    }
}
