using DowndetectorMCP.API.Exceptions;
using DowndetectorMCP.API.Models;
using Microsoft.Playwright;
using System.Globalization;

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

#if DEBUG
            await page.ScreenshotAsync(new() { Path = "C:\\tmp\\screenshot.png" });
#endif
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


        public async Task<ServiceStatusResult> GetServiceStatus(string technicalName, bool includeHistoricalReportData)
        {
            // Init Playwright and Browser
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
            var page = await browser.NewPageAsync(new BrowserNewPageOptions { UserAgent = UserAgent });

            // Get the service status page url according to the service name and country
            var statusPageUrl = this.ServiceStatusUrl(technicalName);

            // Navigate to the status page url
            var response = await page.GotoAsync(statusPageUrl, new PageGotoOptions() { WaitUntil = WaitUntilState.Load });

#if DEBUG
            await page.ScreenshotAsync(new() { Path = "C:\\tmp\\screenshot.png" });
#endif

            // Check if on the 404 page
            if(response != null && response.Status == 404)
            {
                await CloseBrowser(browser, page);
                throw new ServiceNotFoundException(technicalName);
            }

            // Check if blocked by Cloudflare
            if (await TryBypassClouflare(page))
            {
                await CloseBrowser(browser, page);
                throw new RateLimitException();
            }

            var serviceStatusResult = new ServiceStatusResult();

            // Extract JS data
            var jsDataObj = await page.EvaluateAsync<object>(@"
                () => {
                    if (window.DD && window.DD.currentServiceProperties) {
                        return window.DD.currentServiceProperties;
                    }
                    return null;
                }
            ");

            // deserialize the JS data into a CurrentServiceProperties model
            var jsonString = System.Text.Json.JsonSerializer.Serialize(jsDataObj);
            var jsData = System.Text.Json.JsonSerializer.Deserialize<CurrentServiceProperties>(jsonString);

            if (jsData == null)
            {
                await CloseBrowser(browser, page);
                throw new DataNotFoundException(technicalName);
            }

            serviceStatusResult.ServiceName = jsData.Company;
            serviceStatusResult.Status = ParseCompanyStatusToEnum(jsData.Status);

            // Get Reports data (baseline and current reports)
            var historicalReportData = new List<ChartPoint>();

            for (var pointIndex = 0; pointIndex < jsData.Series.Baseline.Data.Count; pointIndex++)
            {
                var baselinePoint = jsData.Series.Baseline.Data[pointIndex];
                var reportPoint = jsData.Series.Reports.Data[pointIndex];

                historicalReportData.Add(new ChartPoint
                {
                    Time = DateTime.ParseExact(reportPoint.X, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                    Report = reportPoint.Y,
                    Baseline = baselinePoint.Y,
                });
            }

            if(includeHistoricalReportData)
                serviceStatusResult.ReportData = historicalReportData;

            serviceStatusResult.LastReportData = historicalReportData.Last();


            // Get most reported issues
            var mostReportedIssues = new List<MostReportedIssue>();
            var indicatorChartPercentages = await page.Locator(".indicatorChart_percentage").AllAsync();
            var indicatorChartNames = await page.Locator(".indicatorChart_name").AllAsync();

            for (var i = 0; i < indicatorChartNames.Count; i++)
            {
                var issueName = await indicatorChartNames[i].InnerTextAsync();
                var issuePercentageText = await indicatorChartPercentages[i].InnerTextAsync();

                if (int.TryParse(issuePercentageText.TrimEnd('%'), out var issuePercentage))
                {
                    mostReportedIssues.Add(new MostReportedIssue
                    {
                        Issue = issueName.Trim(),
                        Percentage = issuePercentage,
                    });
                }
            }

            serviceStatusResult.MostReportedIssues = mostReportedIssues;

            return serviceStatusResult;
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
