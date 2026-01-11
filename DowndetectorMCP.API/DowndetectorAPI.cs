using DowndetectorMCP.API.Exceptions;
using DowndetectorMCP.API.Models;
using Microsoft.Playwright;

namespace DowndetectorMCP.API
{
    public class DowndetectorAPI
    {
        public string BaseUrl { get; set; }
        private const string UserAgent = "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

        public DowndetectorAPI(string countryCode = "us")
        {
            this.BaseUrl = Utils.ApiUrl.GetUrlByCountryCode(countryCode.ToUpper());
        }

        public async Task<SearchServiceResult> SearchService(string serviceName)
        {
            // Init Playwright and Browser
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
            var page = await browser.NewPageAsync(new BrowserNewPageOptions { UserAgent =  UserAgent });

            // Get the search url according to the service name and country
            var searchUrl = this.SearchUrl(serviceName);

            // Navigate to the search url
            await page.GotoAsync(searchUrl, new PageGotoOptions(){ WaitUntil = WaitUntilState.Load });

            await page.ScreenshotAsync(new() { Path = "C:\\tmp\\screenshot.png" });

            // Case : Search results page, no redirection in service page
            if (page.Url == searchUrl || page.Url.StartsWith(searchUrl))
            {
                // Retrieve company cards
                var cards = await page.Locator("div.company-card").AllAsync();

                // If no results found, throw exception
                if (cards.Count == 0)
                    throw new NoResultException(serviceName);

                var searchResult = new SearchServiceResult
                {
                    SearchWord = serviceName,
                    Results = new List<SearchResultItem>(),
                };

                // Extract results
                foreach (var card in cards)
                {
                    var link = card.Locator("a");
                    var foundServiceName = await card.Locator("a .caption h5").InnerTextAsync();
                    var foundServiceUrl = CleanUrl(await link.GetAttributeAsync("href") ?? string.Empty);
                    var foundTechnicalName = CleanTechnicalName(foundServiceUrl);

                    if (foundServiceName != null && link != null)
                    {
                        searchResult.Results.Add(new SearchResultItem
                        {
                            ServiceName = foundServiceName.Trim(),
                            TechnicalName = foundServiceName,
                            Url = foundServiceUrl,
                        });
                    }
                }

                return searchResult;
            }

            return new SearchServiceResult();
        }

        public async Task<ServiceStatusResult> GetServiceStatus(string technicalName)
        {
            return new ServiceStatusResult();
        }

        private string SearchUrl(string serviceName)
        {
            return $"{this.BaseUrl}/search/?q={serviceName.Replace(" ", "+")}";
        }

        private static string CleanUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
        }

        private static string CleanTechnicalName(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // The service technical name is usually the last segment before the final slash
            // Ex: /statut/microsoft-365/ -> microsoft-365
            return segments.Length > 0 ? segments[^1] : string.Empty;
        }
    }
}
