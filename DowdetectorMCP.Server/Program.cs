using DowdetectorMCP.Server.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Services registration
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IServiceSearchCache>(sp =>
    new ServiceSearchCache(
        sp.GetRequiredService<IMemoryCache>(),
        TimeSpan.FromHours(24)
    ));
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();


app.MapGet("/", () => "MCP Downdetector Server is running.");
app.MapMcp("/mcp");

await app.RunAsync();
