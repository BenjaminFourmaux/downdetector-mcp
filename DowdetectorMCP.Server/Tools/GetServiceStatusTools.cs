using DowndetectorMCP.API;
using DowndetectorMCP.API.Exceptions;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DowdetectorMCP.Server.Tools
{
    [McpServerToolType]
    [Description("Get comprehensive status information for an online service, including current status, report counts compared to baseline, 24-hour historical data, and most reported outage causes")]
    public static class GetServiceStatusTools
    {
        [McpServerTool]
        [Description("Get comprehensive status information for an online service, including current status, report counts compared to baseline, 24-hour historical data, and most reported outage causes")]
        public static async Task<string> GetServiceStatus(
            [Description("Name of the service")] string serviceName,
            [Description("The technical service name (slug)")] string technicalServiceName,
            [Description("The country in which we want to know the status of the service")] string country,
            [Description("Include or not the last 24 hours status series (4 points per hour, total 96). By default, false")] bool includeHistoricalReportData = false)
        {
            try
            {
                var downdetectorAPI = new DowndetectorAPI(country);

                var serviceData = await downdetectorAPI.GetServiceStatus(serviceName, technicalServiceName, false);

                return serviceData.ToToon();
            }
            catch (ServiceNotFoundException ex)
            {
                return ex.Message;
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
