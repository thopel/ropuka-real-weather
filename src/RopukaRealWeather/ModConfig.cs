using BepInEx.Configuration;

namespace RopukaRealWeather;

public sealed class ModConfig
{
    public ConfigEntry<bool> Enabled { get; private set; } = null!;
    public ConfigEntry<double> Latitude { get; private set; } = null!;
    public ConfigEntry<double> Longitude { get; private set; } = null!;
    public ConfigEntry<int> RefreshIntervalMinutes { get; private set; } = null!;
    public ConfigEntry<bool> SyncOnStartup { get; private set; } = null!;
    public ConfigEntry<bool> UseRealDayNight { get; private set; } = null!;
    public ConfigEntry<bool> DebugLogging { get; private set; } = null!;

    public static ModConfig Bind(ConfigFile file)
    {
        return new ModConfig
        {
            Enabled = file.Bind("General", "Enabled", true, "Enable real-weather synchronization."),
            Latitude = file.Bind("Location", "Latitude", 49.44024161964049, "Latitude in degrees [-90, 90]."),
            Longitude = file.Bind("Location", "Longitude", 1.095072045140073, "Longitude in degrees [-180, 180]."),
            RefreshIntervalMinutes = file.Bind("Network", "RefreshIntervalMinutes", 15, new ConfigDescription("Minutes between requests.", new AcceptableValueRange<int>(1, 1440))),
            SyncOnStartup = file.Bind("General", "SyncOnStartup", true, "Synchronize as soon as the game weather manager is ready."),
            UseRealDayNight = file.Bind("General", "UseRealDayNight", false, "Reserved: this game build exposes no controllable day/night system."),
            DebugLogging = file.Bind("General", "DebugLogging", false, "Enable verbose diagnostic logging.")
        };
    }

    public static bool CoordinatesAreValid(double latitude, double longitude) =>
        latitude >= -90.0 && latitude <= 90.0 && longitude >= -180.0 && longitude <= 180.0;

    public static bool CoordinatesAreUnset(double latitude, double longitude) => latitude == 0.0 && longitude == 0.0;
}
