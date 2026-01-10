using System;
using System.Collections.Generic;
using System.Text;

namespace DowndetectorMCP.API.Models
{
    public class SearchServiceResult
    {
        public string SearchWord { get; set; } = string.Empty;

        public List<SearchResultItem> Results { get; set; } = new List<SearchResultItem>();
    }

    public class SearchResultItem
    {
        public string ServiceName { get; set; } = string.Empty;
        public string TechnicalName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
