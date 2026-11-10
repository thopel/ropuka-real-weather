# Ropuka Real Weather

An unofficial BepInEx mod for **Ropuka's Idle Island** that periodically applies real-world weather from [Open-Meteo](https://open-meteo.com/). It requires no API key, external application, or private server.

## Important status

- Windows x64 and the Unity Mono build of the game are supported.
- Tested with Unity `6000.0.72f1` and BepInEx `5.4.23.6` Mono x64.
- The mod does **not** replace or edit any original game file, save file, or native setting.
- Coordinates are configured in the BepInEx configuration file. An in-game settings UI is intentionally not included because runtime modifications to this game's settings hierarchy were not stable enough for a public release.
- This project is unofficial and is not affiliated with the game developer, Steam, BepInEx, or Open-Meteo.

## Installation

1. Download `BepInEx_win_x64_5.4.23.6.zip` from the official [BepInEx v5 releases](https://github.com/BepInEx/BepInEx/releases).
2. Extract BepInEx into the game's installation directory, next to `RopukaIdleIsland.exe`.
3. Start the game once, then close it. BepInEx will create its folders.
4. Download the latest `RopukaRealWeather.zip` release.
5. Extract it into the game directory. The DLL must end up at:

   ```text
   BepInEx/plugins/RopukaRealWeather/RopukaRealWeather.dll
   ```

6. Start and close the game once more so the configuration file is generated.
7. Open:

   ```text
   BepInEx/config/com.ropuka.realweather.cfg
   ```

8. The default `Latitude` and `Longitude` are already set; change them if needed, save the file, and restart the game.

Example:

```ini
[Location]
Latitude = 49.44024161964049
Longitude = 1.095072045140073
```

Both coordinates set to `0.0` disable network requests. Valid ranges are `[-90, 90]` for latitude and `[-180, 180]` for longitude. A zero value on only one axis is valid.

## How it works

- Open-Meteo is queried asynchronously every 15 minutes by default.
- Only the configured coordinates are sent to Open-Meteo.
- Network requests time out after 10 seconds.
- A failed request leaves the current weather unchanged and retries at the next interval.
- The mod calls the game's public weather method and does not patch the game's assemblies or weather scheduler.

| WMO code | Game effect |
|---|---|
| 0–1 | `NatureB` (clear) |
| 2–3 | `WindA` (cloudy) |
| 45, 48 | `WindA` (fog fallback) |
| 51–57 | `RainB` (light rain) |
| 61–67, 80–82 | `RainA` (heavy rain) |
| 71–77, 85–86 | `Snow` |
| 95–99 | `RainA` (thunderstorm fallback) |
| Other | `WindA` |

## Uninstallation

Close the game and delete:

```text
BepInEx/plugins/RopukaRealWeather/RopukaRealWeather.dll
```

You may also delete `BepInEx/config/com.ropuka.realweather.cfg`. No original game file needs to be restored.

## Building from source

Requirements:

- .NET 8 SDK
- A local installation of Ropuka's Idle Island
- BepInEx `5.4.23.6` Mono x64 extracted locally

The repository does not contain or redistribute game assemblies or BepInEx binaries. Run:

```powershell
.\scripts\build.ps1 `
  -GameDir "C:\Program Files (x86)\Steam\steamapps\common\RopukaIdleIsland" `
  -BepInExDir "C:\path\to\BepInEx"
```

The release archive is written to `artifacts/RopukaRealWeather.zip`.

## Troubleshooting

Check `BepInEx/LogOutput.log` for `Weather synchronized` or an explicit error message. Confirm that you installed the Mono x64 build of BepInEx, not an IL2CPP build.

When reporting a problem, include the mod version, game version, BepInEx version, and the relevant log lines. Do not upload the entire game directory or proprietary game assemblies.

## License

Source code is available under the [MIT License](LICENSE).
