using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Yastik6.Mcp.Tools;

[McpServerToolType]
public sealed class MarketTools(Yastik6Client client)
{
    [McpServerTool(Name = "get_market_prices", ReadOnly = true)]
    [Description("Latest bid/ask for tracked assets (gold, FX, crypto, BIST stocks), plus how fresh each data source is.")]
    public async Task<MarketSummary> GetMarketPrices(
        [Description("Filter by asset type, e.g. METAL, CURRENCY, CRYPTO, STOCK")] string? type = null,
        [Description("Single asset, e.g. GRAM_ALTIN, USDTRY=X, BTC-USD, THYAO.IS")] string? productCode = null,
        CancellationToken ct = default)
    {
        try
        {
            return MarketSummary.From(await client.GetMarketAsync(ct), type, productCode, DateTimeOffset.UtcNow);
        }
        catch (ApiException e)
        {
            // McpException messages reach the model; other exception messages are hidden by the SDK.
            throw new McpException(e.Message);
        }
    }
}
