using Xunit;

namespace RopukaRealWeather.Tests;

public sealed class WeatherMapperTests
{
    [Theory]
    [InlineData(0, GameWeather.Clear)]
    [InlineData(1, GameWeather.Clear)]
    [InlineData(3, GameWeather.Cloudy)]
    [InlineData(45, GameWeather.Fog)]
    [InlineData(51, GameWeather.LightRain)]
    [InlineData(67, GameWeather.HeavyRain)]
    [InlineData(71, GameWeather.Snow)]
    [InlineData(82, GameWeather.HeavyRain)]
    [InlineData(86, GameWeather.Snow)]
    [InlineData(99, GameWeather.Thunderstorm)]
    public void MapsOfficialWmoRanges(int code, GameWeather expected) => Assert.Equal(expected, WeatherMapper.FromWmoCode(code));

    [Fact]
    public void UnsupportedConditionFallsBackToCloudy() => Assert.Equal(GameWeather.Cloudy, WeatherMapper.FromWmoCode(100));

    [Fact]
    public void IdenticalWeatherRequiresNoChange() => Assert.False(WeatherMapper.ShouldApply(GameWeather.Snow, GameWeather.Snow));
}
