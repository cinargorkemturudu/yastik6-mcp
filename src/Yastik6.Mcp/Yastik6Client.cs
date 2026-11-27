using System.Net;
using System.Net.Http.Json;

namespace Yastik6.Mcp;

public sealed record PriceItem(
    string ProductCode,
    string Name,
    string Type,
    decimal? Bid,
    decimal? Ask,
    decimal? ChangeRate,
    DateTimeOffset AsOfUtc,
    string Source,
    bool IsMarketOpen);

public sealed record MarketSnapshot(IReadOnlyList<PriceItem> Items, DateTimeOffset ComputedAtUtc);

public sealed record PublicApiIndex(string RateLimit, IReadOnlyList<string> ProductCodes);

public sealed record HealthStatus(string Status, int HttpStatus);

public sealed class ApiException(HttpStatusCode status, string message) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}

/// <summary>Thin client for the keyless /api/public tier. It never sends credentials.</summary>
public sealed class Yastik6Client(HttpClient http)
{
    public const string DefaultBaseUrl = "https://yastik6.net";

    public Task<MarketSnapshot> GetMarketAsync(CancellationToken ct = default) =>
        GetJsonAsync<MarketSnapshot>("/api/public/market", ct);

    public Task<PublicApiIndex> GetIndexAsync(CancellationToken ct = default) =>
        GetJsonAsync<PublicApiIndex>("/api/public", ct);

    public async Task<HealthStatus> GetHealthAsync(CancellationToken ct = default)
    {
        using var response = await http.GetAsync("/api/health", ct);
        return new HealthStatus((await response.Content.ReadAsStringAsync(ct)).Trim(), (int)response.StatusCode);
    }

    private async Task<T> GetJsonAsync<T>(string path, CancellationToken ct)
    {
        using var response = await http.GetAsync(path, ct);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new ApiException(response.StatusCode,
                "Rate limited by the public API (30 requests/minute per IP). Try again shortly.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(response.StatusCode, $"API returned {(int)response.StatusCode} for {path}");
        }

        return (await response.Content.ReadFromJsonAsync<T>(ct))!;
    }
}
