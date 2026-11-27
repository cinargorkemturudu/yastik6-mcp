using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Yastik6.Mcp.Tools;

[McpServerToolType]
public sealed class StatusTools(Yastik6Client client)
{
    [McpServerTool(Name = "list_product_codes", ReadOnly = true)]
    [Description("Valid product codes and the public API's rate limit.")]
    public async Task<PublicApiIndex> ListProductCodes(CancellationToken ct = default)
    {
        try
        {
            return await client.GetIndexAsync(ct);
        }
        catch (ApiException e)
        {
            throw new McpException(e.Message);
        }
    }

    [McpServerTool(Name = "get_health", ReadOnly = true)]
    [Description("Liveness of the production API (GET /api/health).")]
    public Task<HealthStatus> GetHealth(CancellationToken ct = default) => client.GetHealthAsync(ct);
}
