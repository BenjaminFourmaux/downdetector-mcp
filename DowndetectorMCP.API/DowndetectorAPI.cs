using DowndetectorMCP.API.Exceptions;
using DowndetectorMCP.API.Models;
using DowndetectorMCP.API.Utils;
using Microsoft.Playwright;
using System.Text.Json;

namespace DowndetectorMCP.API
{
    public class DowndetectorAPI
    {
        public string BaseUrl { get; set; }
        public string CountryCode { get; set; }

        private const string UserAgent = "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

        public DowndetectorAPI(string countryCode = "us")
        {
            this.CountryCode = countryCode.ToUpper();
            this.BaseUrl = ApiUrl.GetUrlByCountryCode(countryCode.ToUpper());
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
            var response = await page.GotoAsync(searchUrl, new PageGotoOptions(){ WaitUntil = WaitUntilState.Load });

#if DEBUG
            await page.ScreenshotAsync(new() { Path = "C:\\tmp\\screenshot.png" });
#endif
            // Check if 404 that mean we are on the new website
            if (response != null && response.Status == 404)
            {
                var searchApiResult = await SearchFromAPI(page, serviceName);
                return ConvertSearchResultToModel(searchApiResult, this.BaseUrl, serviceName);
            }

            // Check if blocked by Cloudflare
            if (await TryBypassClouflare(page))
            {
                await CloseBrowser(browser, page);
                throw new RateLimitException();
            }

            var searchResult = new SearchServiceResult
            {
                SearchWord = serviceName,
                Results = new List<SearchResultItem>(),
            };

            // Case : Search results page, no redirection in service page
            if (page.Url == searchUrl || page.Url.StartsWith(searchUrl))
            {
                // Retrieve company cards
                var cards = await page.Locator("div.company-card").AllAsync();

                // If no results found, throw exception
                if (cards.Count == 0)
                {
                    await CloseBrowser(browser, page);
                    throw new NoResultException(serviceName);
                }

                // Extract results
                foreach (var card in cards)
                {
                    var link = card.Locator("a");
                    var foundServiceName = await card.Locator("a .caption h5").InnerTextAsync();
                    var foundServiceUrl = CleanUrl(this.BaseUrl + (await link.GetAttributeAsync("href") ?? string.Empty));
                    var foundTechnicalName = ExtractTechnicalServiceNameFromURL(foundServiceUrl);

                    if (foundServiceName != null && link != null)
                    {
                        searchResult.Results.Add(new SearchResultItem
                        {
                            ServiceName = foundServiceName.Trim(),
                            TechnicalName = foundTechnicalName,
                            Url = foundServiceUrl,
                        });
                    }
                }

                await CloseBrowser(browser, page);
                return searchResult;
            }
            else // Redirect onto the service status page
            {
                var cleanUrl = CleanUrl(page.Url);
                var foundTechnicalName = ExtractTechnicalServiceNameFromURL(cleanUrl);
                var foundServiceName = await page.Locator(".breadcrumb-item.active").InnerTextAsync();

                searchResult.Results.Add(new SearchResultItem
                {
                    ServiceName = foundServiceName.Trim(),
                    TechnicalName = foundTechnicalName,
                    Url = cleanUrl,
                });

                await CloseBrowser(browser, page);
                return searchResult;
            }
        }


        public async Task<ServiceStatusResult> GetServiceStatus(string serviceName, string slug, bool includeHistoricalReportData)
        {
            // Init Playwright and Browser
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
            var page = await browser.NewPageAsync(new BrowserNewPageOptions { UserAgent = UserAgent });

            // Navigate to the home page
            var response = await page.GotoAsync(this.BaseUrl, new PageGotoOptions() { WaitUntil = WaitUntilState.Load });

#if DEBUG
            await page.ScreenshotAsync(new() { Path = "C:\\tmp\\screenshot.png" });
#endif

            // Check if blocked by Cloudflare
            if (await TryBypassClouflare(page))
            {
                await CloseBrowser(browser, page);
                throw new RateLimitException();
            }

            // Extract status from search API query
            var results = await SearchFromAPI(page, serviceName);

            var serviceResult = results.FirstOrDefault(r => r.Company.Slug == slug);

            if (serviceResult != null)
            {
                var serviceStatus = new ServiceStatusResult();

                serviceStatus.ServiceName = serviceName;
                serviceStatus.Status = ParseCompanyStatusToEnum(serviceResult.Company.Stats.Status);

                return serviceStatus;
            }
            else
            {
                throw new DataNotFoundException(slug);
            }
        }

        public async Task<List<SearchResultType>> SearchFromAPI(IPage page, string searchWord)
        {
            var local = $"{this.CountryCode.ToLower()}-{this.CountryCode.ToUpper()}";

            // Call the API to the /search endpoint
            /*var response = await this.httpClient.GetAsync(ApiUrl.Url + $"/search/?q={Uri.EscapeDataString(searchWord)}&local={local}");

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();*/

            // Note: I cannot get data from Downdetector API via HttpClient (passing cookies and user-agent not working)
            // so we need to goto the API endpoint with Playwright browser and extract row data (d'où le IPage en paramètre)

            var response = await page.GotoAsync(ApiUrl.Url + $"/search/?q={Uri.EscapeDataString(searchWord)}&local={local}");

            var jsonContent = await response.TextAsync();

            var data = JsonSerializer.Deserialize<List<SearchResultType>>(jsonContent);

            return data ?? new List<SearchResultType>();
        }


        #region Private Methods
        private string SearchUrl(string serviceName)
        {
            return $"{this.BaseUrl}/search/?q={serviceName.Replace(" ", "+")}";
        }

        private string ServiceStatusUrl(string technicalName)
        {
            return $"{this.BaseUrl}/status/{technicalName.ToLower()}/";
        }

        /// <summary>
        /// Remove the '?_gl' URL parameter in the Downdetector URLs
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <remarks>
        /// input : https://downdetector.fr/statut/youtube/?_gl=1*H1Z1....
        /// <br/>
        /// output : https://downdetector.fr/statut/youtube/
        /// </remarks>
        private static string CleanUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
        }

        private static string ExtractTechnicalServiceNameFromURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return segments.Length > 0 ? segments[^1] : string.Empty;
        }

        private static SearchServiceResult ConvertSearchResultToModel(List<SearchResultType> data, string url, string searchWord)
        {
            var searchServiceResult = new SearchServiceResult();

            searchServiceResult.SearchWord = searchWord;

            foreach (var item in data)
            {
                searchServiceResult.Results.Add(new SearchResultItem
                {
                    ServiceName = item.Company.Name,
                    TechnicalName = item.Company.Slug,
                    Url = url + "/status/" + item.Company.Slug,
                    Category = item.Company.Category.Slug.Replace('-', ' ')
                });
            }
            return searchServiceResult;
        }

        private static ServiceStatus ParseCompanyStatusToEnum(string status)
        {
            return status switch
            {
                "success" => ServiceStatus.SUCCESS,
                "warning" => ServiceStatus.WARNING,
                "danger" => ServiceStatus.DANGER,
                _ => ServiceStatus.SUCCESS
            };
        }

        private static async Task CloseBrowser(IBrowser browser, IPage page)
        {
            await page.CloseAsync();
            await browser.CloseAsync();
        }

        private static async Task<bool> IsCloudflare(IPage page)
        {
            try
            {
                var cloudflareLink = page.Locator("a:has-text('Cloudflare')");
                var count = await cloudflareLink.CountAsync();
                
                if (count == 0)
                    return false;
                    
                var href = await cloudflareLink.First.GetAttributeAsync("href");
                return href != null && href.StartsWith("https://www.cloudflare.com");
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> TryBypassClouflare(IPage page)
        {
            if (await IsCloudflare(page))
            {
                await page.WaitForTimeoutAsync(3000);

                // Your are a humman, trust me and click on the checkbox
                //await page.GetByRole(AriaRole.Checkbox).ClickAsync();
                //await page.WaitForTimeoutAsync(1000);

                if (await IsCloudflare(page)) 
                {
                    return true;
                }
                return false;
            }
            return false;
        } 
        #endregion
    }
}
