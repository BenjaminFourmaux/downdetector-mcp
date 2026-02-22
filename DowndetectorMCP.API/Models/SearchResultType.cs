using System.Text.Json.Serialization;

namespace DowndetectorMCP.API.Models
{
    public class SearchResultType
    {
        [JsonPropertyName("company")]
        public CompanyType Company { get; set; }
    }

    public class CompanyType
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("logo")]
        public string Logo { get; set; }

        [JsonPropertyName("category")]
        public CategoryType Category { get; set; }

        [JsonPropertyName("stats")]
        public CompanyStatsType Stats { get; set; }
    }

    public class CategoryType
    {
        [JsonPropertyName("slug")]
        public string Slug { get; set; }
    }

    public class CompanyStatsType
    {
        [JsonPropertyName("stats")]
        public string Status { get; set; }

        [JsonPropertyName("sparkline")]
        public int[] Sparkline { get; set; }
    }
}
