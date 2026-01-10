using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DowdetectorMCP.Server.Tools
{
    [McpServerToolType]
    [Description("Search for the technical service name for getting right service status")]
    public static class SearchServiceNameTools
    {
        [McpServerTool]
        [Description("Search for the technical service name for getting right service status")]
        public static async Task<string> SearchServiceName(
            [Description("The service name")] string serviceName,
            [Description("The country in which we want to know the status of the service")] string localization)
        {
            return "google-gemini";
        }
    }
}
