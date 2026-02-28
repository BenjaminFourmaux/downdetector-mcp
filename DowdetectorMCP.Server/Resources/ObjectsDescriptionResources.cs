using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DowdetectorMCP.Server.Resources
{
    [McpServerResourceType]
    [Description("Get objects description")]
    public class ObjectsDescriptionResources
    {
        [McpServerResource]
        [Description("Get object description for result of the SearchServiceName tool")]
        public string ObjectDescriptionSearchServiceResult()
        {
            return """
                Result object of the SearchServiceName tool wich contains the service search result

                ## SearchServiceResult
                - SearchWord: Term of research
                - Results: List of SearchResultItem which contains the services found for the research

                ## SearchResultItem
                - ServiceName: The real service name
                - TechnicalName: The name of the service used to retrieve service status
                - Url: The Donwdetector service URL
                - Category: Name of the service catgory
                """;
        }

        [McpServerResource]
        [Description("Get object description for result of the tool GetServiceStatus")]
        public string ObjectDescriptionGetServiceStatus()
        {
            return """
                Result object of the GetServiceStatus toll wich contains data about the liveness and status of the service

                ## ServiceStatusResult
                - ServiceName: The real service name
                - Status: Enum of the current service status. Can be on of:
                  - SUCCESS: The service doesn't seem to have any problems (Healthy)
                  - WARNING: Some people report an outage. The service is degraded
                  - DANGER: Many people reporte an outage. The service is down (Unhealthy)
                - MostReportedIssues: List of MostReportedIssue wich contains the top 3 of the most reported service issue
                - ReportData: List of ChartPoint, data points wich represent the number of report/baseline in the last 24H. Is optional if the parameter `includeHistoricalReportData` not set to true (false by default)
                - LastReportData: ChartPoint of the last data point

                ## MostReportedIssue
                - Issue: Name of the issue (e.q. app connection, online game service, website...)
                - Percentage: Percentage of the issue reported by user

                ## ChartPoint
                - Time: DateTime offset in format dd-MM-yyyyTHH:mm:ss
                - Report: Number of user reports
                - Baseline: Number of the report baseline
                """;
        }
    }
}
