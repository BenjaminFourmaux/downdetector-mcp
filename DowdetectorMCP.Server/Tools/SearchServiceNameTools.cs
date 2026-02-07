using DowdetectorMCP.Server.Services;
using DowndetectorMCP.API;
using DowndetectorMCP.API.Exceptions;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DowdetectorMCP.Server.Tools
{
    [McpServerToolType]
    [Description("Search for the technical service name for getting right service status")]
    public class SearchServiceNameTools
    {
        private readonly IServiceSearchCache _cache;

        public SearchServiceNameTools(IServiceSearchCache cache)
        {
            _cache = cache;
        }

        [McpServerTool]
        [Description("Search for the technical service name for getting right service status")]
        public async Task<string> SearchServiceName(
            [Description("The service name")] string serviceName,
            [Description("The country alpha2 code in which we want to know the status of the service")] string country)
        {
            // Check cache first
            if (_cache.TryGetValue(serviceName, country, out var cachedResult))
            {
                return cachedResult!.ToToon();
            }

            try
            {
                var downdetectorAPI = new DowndetectorAPI(country);

                var searchResult = await downdetectorAPI.SearchService(serviceName);

                // Set the result in cache
                _cache.Set(serviceName, country, searchResult);

                return searchResult.ToToon();
            }
            catch (NoResultException ex)
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
