using System.Net;

namespace Yastik6.Mcp.Tests;

public class Yastik6ClientTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequest = request;
            return Task.FromResult(respond(request));
        }
    }

    private static Yastik6Client ClientFor(StubHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://example.test") });

    [Fact]
    public async Task Rate_limit_becomes_a_readable_error()
    {
        var client = ClientFor(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)));

        var error = await Assert.ThrowsAsync<ApiException>(() => client.GetMarketAsync());

        Assert.Equal(HttpStatusCode.TooManyRequests, error.Status);
        Assert.Contains("Rate limited", error.Message);
    }

    [Fact]
    public async Task Sends_no_credentials()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"items":[],"computedAtUtc":"2026-01-01T00:00:00Z"}""", System.Text.Encoding.UTF8, "application/json"),
        });

        await ClientFor(handler).GetMarketAsync();

        Assert.Null(handler.LastRequest!.Headers.Authorization);
        Assert.False(handler.LastRequest.Headers.Contains("X-Mobile-Client-Key"));
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
    }

    [Fact]
    public async Task Health_returns_body_and_status()
    {
        var client = ClientFor(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Healthy\n") }));

        var health = await client.GetHealthAsync();

        Assert.Equal(new HealthStatus("Healthy", 200), health);
    }
}
