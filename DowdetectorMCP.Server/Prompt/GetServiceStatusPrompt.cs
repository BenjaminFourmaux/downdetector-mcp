using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DowdetectorMCP.Server.Prompt
{
    [McpServerPromptType]
    [Description("Guide for using this MCP tools")]
    public class GetServiceStatusPrompt
    {
        [McpServerPrompt]
        [Description("Guide for using this MCP tools")]
        public PromptMessage AskServiceSatatus()
        {
            var prompt = """
                ## General purprose
                This MCP used data from the website Downdetector to provide data for AI Agents about the status of online services.
                
                Downdector is a platform wich allow users to report an outage on service (user reporting). 
                Most users reported, furthermore, this seems to indicate a problem with a service.

                There are many instance of Downdetector website across the world (one website per country). 
                Some services doesn't exist in certain country and vice-verca.
                That's why you need to specified the country (the alpha-2 code) in which, the user want to know the service status.

                ## How to use
                If you want to know the status of an online service, you need to call these tools in order.
                
                ### 1. SearchServiceName
                Downdetector used specific service name in backend (called `technicalServiceName`) and you need to search on a service with this tool for retrieve the correct technicalServiceName.
                
                You receive a list of search result service.

                ### 2. GetServiceStatus
                Once you have the `technicalServiceName` you can call this second tool to retrieve the status and data about service.
                
                If the user want to analyse the latest 24H data of this service, you must ste the parameter `includeHistoricalReportData` to `true`, for retrieve historical data chart point (number of user's report and baseline at datetime)
                """;
            return new PromptMessage { Role = Role.Assistant, Content = new TextContentBlock() { Text = prompt } };
        }
    }
}
