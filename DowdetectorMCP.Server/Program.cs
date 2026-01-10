var builder = WebApplication.CreateBuilder(args);

// Services registration
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();


app.MapGet("/", () => "MCP Downdetector Server is running.");
app.MapMcp("/mcp");

await app.RunAsync();
