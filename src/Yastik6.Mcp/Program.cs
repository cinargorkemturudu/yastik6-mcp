using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yastik6.Mcp;

var builder = Host.CreateApplicationBuilder(args);

// stdout carries the MCP protocol itself, so all logging has to go to stderr.
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// YASTIK6_BASE_URL lets the same server point at a local backend (http://localhost:5179).
var baseUrl = Environment.GetEnvironmentVariable("YASTIK6_BASE_URL") ?? Yastik6Client.DefaultBaseUrl;

builder.Services.AddHttpClient<Yastik6Client>(http =>
{
    http.BaseAddress = new Uri(baseUrl);
    http.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
