using System.Text.Json.Serialization;

namespace DowndetectorMCP.API.Models
{
    /// <summary>
    /// The C# reprensentation of the JSON object returned by Downdetector inside the JS variable "window.DD.currentServiceProperties"
    /// </summary>
    public class CurrentServiceProperties
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("communicate")]
        public bool? Communicate { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; } = string.Empty;

        [JsonPropertyName("indicatorToken")]
        public string IndicatorToken { get; set; } = string.Empty;

        /// <summary>
        /// The maximum number of user reports in the last 24 hours for this service.
        /// </summary>
        [JsonPropertyName("max")]
        public int Max { get; set; }

        /// <summary>
        /// The maximum baseline of user reports in the last 24 hours for this service.
        /// </summary>
        [JsonPropertyName("max_baseline")]
        public int MaxBaseline { get; set; }


        /// <summary>
        /// The minimum baseline of user reports in the last 24 hours for this service.
        /// </summary>
        [JsonPropertyName("min_baseline")]
        public int MinBaseline { get; set; }

        [JsonPropertyName("regionalCommunicate")]
        public bool? RegionalCommunicate { get; set; }

        public List<RelatedCompany> RelatedCompanies { get; set; } = new List<RelatedCompany>();

        [JsonPropertyName("series")]
        public Series Series { get; set; } = new Series();

        /// <summary>
        /// The status of the service, can be "success", "warning" or "danger"
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class RelatedCompany
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The Downdetector domain name of the geo instance
        /// </summary>
        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// The status page url 
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The Alpha-2 country code of the geo instance
        /// </summary>
        [JsonPropertyName("countryIso2")]
        public string CountryIso2 { get; set; } = string.Empty;
    }

    public class Series
    {
        [JsonPropertyName("baseline")]
        public SeriesItem Baseline { get; set; } = new SeriesItem();

        [JsonPropertyName("reports")]
        public SeriesItem Reports { get; set; } = new SeriesItem();
    }

    public class SeriesItem
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<SeriesData> Data { get; set; } = new List<SeriesData>();
    }

    public class SeriesData
    {
        /// <summary>
        /// The X axis is the timestamp of the report, in ISO 8601 format (e.g. "2026-02-06T14:03:06+00:00")
        /// </summary>
        [JsonPropertyName("x")]
        public string X { get; set; } = string.Empty;

        /// <summary>
        /// The Y axis is the number of reports (baseline or user reports)
        /// </summary>
        [JsonPropertyName("y")]
        public int Y { get; set; }
    }
}
