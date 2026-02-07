using DowndetectorMCP.API;
using DowndetectorMCP.API.Exceptions;
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
            [Description("The country in which we want to know the status of the service")] string country,
            [Description("Include or not the last 24 hours status series (4 points per hour, total 96). By default, false")] bool includeHistoricalReportData = false)
        {
            try
            {
                var downdetectorAPI = new DowndetectorAPI(country);

                var serviceData = await downdetectorAPI.GetServiceStatus(technicalServiceName, includeHistoricalReportData);

                return serviceData.ToToon();
            }
            catch (DataNotFoundException ex)
            {
                return ex.Message;
            }
            catch (RateLimitException ex)
            {
                return ex.Message;
            }
        }
    }
}
