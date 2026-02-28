using DowndetectorMCP.Shared.Extensions;

namespace DowndetectorMCP.API.Models
{
    public class SearchServiceResult
    {
        public string SearchWord { get; set; } = string.Empty;

        public List<SearchResultItem> Results { get; set; } = new List<SearchResultItem>();

        /// <summary>
        /// Return the string Toon format representation of the search result.
        /// <see href="https://github.com/toon-format/toon"/>
        /// </summary>
        /// <returns></returns>
        public string ToToon()
        {
            return ToonConverter.ToToon(this);
        }
    }

    public class SearchResultItem
    {
        public string ServiceName { get; set; } = string.Empty;
        public string TechnicalName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Category {  get; set; } = string.Empty;
    }
}
