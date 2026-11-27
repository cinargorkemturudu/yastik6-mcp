namespace Yastik6.Mcp;

public sealed record SourceFreshness(string Source, DateTimeOffset NewestAsOfUtc, long AgeSeconds);

public sealed record MarketSummary(
    DateTimeOffset ComputedAtUtc,
    int Count,
    IReadOnlyList<PriceItem> Items,
    IReadOnlyList<SourceFreshness> Sources)
{
    /// <summary>
    /// Filters a snapshot and adds per-source freshness. Freshness is computed over the whole snapshot,
    /// so filtering never hides a stale provider. <paramref name="now"/> is injectable for tests.
    /// </summary>
    public static MarketSummary From(
        MarketSnapshot snapshot, string? type, string? productCode, DateTimeOffset now)
    {
        var items = snapshot.Items
            .Where(i => type is null || i.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .Where(i => productCode is null || i.ProductCode == productCode)
            .ToList();

        var sources = snapshot.Items
            .GroupBy(i => i.Source)
            .Select(g =>
            {
                var newest = g.Max(i => i.AsOfUtc);
                return new SourceFreshness(g.Key, newest, Math.Max(0, (long)(now - newest).TotalSeconds));
            })
            .ToList();

        return new MarketSummary(snapshot.ComputedAtUtc, items.Count, items, sources);
    }
}
