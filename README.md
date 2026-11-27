# yastik6-mcp

[![CI](https://github.com/cinargorkemturudu/yastik6-mcp/actions/workflows/ci.yml/badge.svg)](https://github.com/cinargorkemturudu/yastik6-mcp/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4.svg)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A read-only [Model Context Protocol](https://modelcontextprotocol.io) server that lets an AI assistant query the live [Yastık6](https://github.com/cinargorkemturudu/Yastik6-Portfolio-Showcase) market-data API: gold, currencies, crypto and BIST stocks.

It talks only to the keyless public tier (`https://yastik6.net/api/public`), so there is nothing to configure and no secret to leak.

## Tools

| Tool | What it returns |
|---|---|
| `get_market_prices` | Latest bid/ask, optionally filtered by `type` or `productCode`, plus the age of each data source so a stale provider stands out |
| `list_product_codes` | Valid product codes and the API's rate limit |
| `get_health` | Liveness of the production API |

Every tool is annotated `readOnlyHint`, and the server only ever issues `GET` requests.

## Use it with Claude Code

```bash
git clone https://github.com/cinargorkemturudu/yastik6-mcp.git
cd yastik6-mcp && dotnet build -c Release src/Yastik6.Mcp
claude mcp add yastik6 -- dotnet "$(pwd)/src/Yastik6.Mcp/bin/Release/net10.0/Yastik6.Mcp.dll"
```

Then ask: *"Which Yastık6 data source is the stalest right now?"*

Set `YASTIK6_BASE_URL=http://localhost:5179` to point it at a [local backend](https://github.com/cinargorkemturudu/yastik6-backend) instead.

## Security model

- **Public tier only.** The client never sends a credential, and a test asserts that.
- **Read-only.** There are no tools that write, deploy or restart anything.
- **Polite.** The public API allows 30 requests/minute per IP. A `429` becomes a readable tool error instead of a retry loop.
- **Bounded.** Every request has a 10-second timeout.
- **Quiet protocol channel.** stdout carries the MCP protocol, so all logging goes to stderr.

## Development

```bash
dotnet test
```

Built on .NET 10 with the official [`ModelContextProtocol`](https://www.nuget.org/packages/ModelContextProtocol) C# SDK, over stdio. Tools are plain methods marked `[McpServerTool]`; the SDK builds their schemas from the signatures and `[Description]` attributes. `Yastik6Client` is a typed `HttpClient`, and `MarketSummary` holds the pure filtering/freshness logic, tested with a stubbed `HttpMessageHandler`.

## License

MIT
