# Changelog

All notable changes to this project are documented here.

## 2.0.3

- Keep the game effect synchronized with the latest Open-Meteo condition when the game's random weather scheduler attempts to replace it.
- Map slight and moderate rain (WMO 61 and 63) and slight rain showers (WMO 80) to the light-rain effect.
- Continue to use the reported WMO condition as the sole input for selecting snow; temperature does not override the condition.
- Add the missing BepInEx reference to the test project.

## 2.0.1

- Added asynchronous Open-Meteo synchronization with a configurable refresh interval.
- Added validated latitude and longitude configuration.
- Added WMO-to-game weather mapping and automated tests.
- Removed runtime patches and experimental in-game settings UI code.
- Added a non-invasive, distributable BepInEx package.

