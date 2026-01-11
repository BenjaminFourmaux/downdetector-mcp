using DowndetectorMCP.API;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DowdetectorMCP.Server.Tools
{
    [McpServerToolType]
    [Description("Get the status informations of an online service")]
    public static class GetServiceStatusTools
    {
        [McpServerTool]
        [Description("Get the status informations of an online service")]
        public static async Task<string> GetServiceStatus(
            [Description("The technical service name")] string technicalServiceName,
            [Description("The country in which we want to know the status of the service")] string country)
        {
            var downdetectorAPI = new DowndetectorAPI(country);

            var service = await downdetectorAPI.GetServiceStatus(technicalServiceName);

            return "google-gemini";
        }
    }
}
