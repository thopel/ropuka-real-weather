using System;
using System.IO;
using BepInEx.Configuration;
using Xunit;

namespace RopukaRealWeather.Tests;

public sealed class ModConfigTests
{
    [Fact]
    public void UsesRequestedCoordinatesAsDefaults()
    {
        var configPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.cfg");
        var config = ModConfig.Bind(new ConfigFile(configPath, false));

        Assert.Equal(49.44024161964049, config.Latitude.Value);
        Assert.Equal(1.095072045140073, config.Longitude.Value);
    }

    [Theory]
    [InlineData(-90, -180, true)]
    [InlineData(90, 180, true)]
    [InlineData(0, 2, true)]
    [InlineData(45, 0, true)]
    [InlineData(91, 0, false)]
    [InlineData(0, -181, false)]
    public void ValidatesCoordinateRanges(double lat, double lon, bool expected) => Assert.Equal(expected, ModConfig.CoordinatesAreValid(lat, lon));

    [Fact]
    public void OnlyBothZeroMeansUnset()
    {
        Assert.True(ModConfig.CoordinatesAreUnset(0, 0));
        Assert.False(ModConfig.CoordinatesAreUnset(0, 1));
        Assert.False(ModConfig.CoordinatesAreUnset(1, 0));
    }
}
