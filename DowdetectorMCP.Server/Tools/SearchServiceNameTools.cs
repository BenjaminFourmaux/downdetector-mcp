using DowndetectorMCP.API;
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
            [Description("The country alpha2 code in which we want to know the status of the service")] string localization)
        {
            var downdetectorAPI = new DowndetectorAPI(localization);

            var searchResult = await downdetectorAPI.SearchService(serviceName);

            return "google-gemini";
        }
    }
}
