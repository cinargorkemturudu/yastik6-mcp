namespace Yastik6.Mcp.Tests;

public class MarketSummaryTests
{
    private static PriceItem Item(string code, string type, string source, string asOf) =>
        new(code, code, type, 1m, 2m, 0m, DateTimeOffset.Parse(asOf), source, true);

    private static readonly MarketSnapshot Snapshot = new(
        [
            Item("GRAM_ALTIN", "METAL", "Harem", "2026-01-01T00:00:00Z"),
            Item("AYAR_22", "METAL", "Harem", "2026-01-01T00:00:05Z"),
            Item("BTC-USD", "CRYPTO", "Binance", "2025-12-31T23:00:00Z"),
        ],
        DateTimeOffset.Parse("2026-01-01T00:00:10Z"));

    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-01-01T00:01:00Z");

    [Fact]
    public void Filters_by_type_case_insensitively()
    {
        var result = MarketSummary.From(Snapshot, "metal", null, Now);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Filters_by_product_code()
    {
        var result = MarketSummary.From(Snapshot, null, "BTC-USD", Now);

        Assert.Equal(["BTC-USD"], result.Items.Select(i => i.ProductCode));
    }

    [Fact]
    public void Reports_newest_timestamp_and_age_per_source()
    {
        var sources = MarketSummary.From(Snapshot, null, null, Now).Sources;

        var harem = Assert.Single(sources, s => s.Source == "Harem");
        Assert.Equal(DateTimeOffset.Parse("2026-01-01T00:00:05Z"), harem.NewestAsOfUtc);
        Assert.Equal(55, harem.AgeSeconds);
        Assert.Equal(3660, Assert.Single(sources, s => s.Source == "Binance").AgeSeconds);
    }

    [Fact]
    public void Filtering_does_not_hide_a_sources_freshness()
    {
        var result = MarketSummary.From(Snapshot, null, "GRAM_ALTIN", Now);

        Assert.Equal(2, result.Sources.Count);
    }
}
