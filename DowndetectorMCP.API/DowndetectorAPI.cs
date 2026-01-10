using DowndetectorMCP.API.Models;
using Microsoft.Playwright;

namespace DowndetectorMCP.API
{
    public class DowndetectorAPI
    {
        public string BaseUrl { get; set; }

        public DowndetectorAPI(string localisation = "us")
        {
            this.BaseUrl = Utils.ApiUrl.GetUrlByCountryCode(localisation.ToUpper());
        }

        public async Task<SearchServiceResult> SearchService(string serviceName)
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
            var page = await browser.NewPageAsync();

            var searchUrl = this.searchUrl(serviceName);

            await page.GotoAsync(searchUrl, new PageGotoOptions(){ WaitUntil = WaitUntilState.Load });

            await page.ScreenshotAsync(new() { Path = "C:\\tmp\\screenshot.png" });

            if (page.Url == searchUrl || page.Url.StartsWith(searchUrl)) // Case : Search results page, no redirection in service page
            {
                // TODO: manage if no results found

                var searchResult = new SearchServiceResult
                {
                    SearchWord = serviceName,
                    Results = new List<SearchResultItem>(),
                };

                var cards = page.Locator(".company-card");

                foreach (var card in await page.Locator(".company-card").AllAsync())
                {
                    var foundServiceName = card.Locator("a .caption h5");
                    var serviceLink = card.Locator("a");
                    if (serviceName != null && serviceLink != null)
                    {
                        searchResult.Results.Add(new SearchResultItem
                        {
                            ServiceName = (await foundServiceName.InnerTextAsync()).Trim(),
                            TechnicalName = "",
                            Url = await serviceLink.GetAttributeAsync("href") ?? string.Empty,
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

        private string searchUrl(string serviceName)
        {
            return $"{this.BaseUrl}/search/?q={Uri.EscapeDataString(serviceName)}";
        }
    }
}
