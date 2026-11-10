using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RopukaRealWeather.Tests;

public sealed class OpenMeteoClientTests
{
    [Fact]
    public async Task ParsesValidResponse()
    {
        using var http = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"current\":{\"weather_code\":61,\"is_day\":1,\"temperature_2m\":12.5}}")
        }));
        using var client = new OpenMeteoClient(http);
        var result = await client.GetCurrentAsync(48.8, 2.3, CancellationToken.None);
        Assert.Equal(61, result.WeatherCode);
        Assert.True(result.IsDay);
    }

    [Fact]
    public async Task RejectsInvalidJson()
    {
        using var http = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("not json") }));
        using var client = new OpenMeteoClient(http);
        await Assert.ThrowsAsync<FormatException>(() => client.GetCurrentAsync(1, 1, CancellationToken.None));
    }

    [Fact]
    public async Task PropagatesNetworkError()
    {
        using var http = new HttpClient(new StubHandler(_ => throw new HttpRequestException("offline")));
        using var client = new OpenMeteoClient(http);
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCurrentAsync(1, 1, CancellationToken.None));
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_handler(request));
    }
}
