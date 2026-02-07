using DowndetectorMCP.API.Utils;

namespace DowndetectorMCP.API.Models
{
    public class ServiceStatusResult
    {
        public string ServiceName { get; set; } = string.Empty;
        public ServiceStatus Status { get; set; } = ServiceStatus.SUCCESS;
        public List<MostReportedIssue> MostReportedIssues { get; set; } = new();
        public List<ChartPoint> ReportData { get; set; } = new();
        public ChartPoint LastReportData { get; set; } = new();

        /// <summary>
        /// Return the string Toon format representation of the service status.
        /// <see href="https://github.com/toon-format/toon"/>
        /// </summary>
        /// <returns></returns>
        public string ToToon()
        {
            return ToonConverter.ToToon(this);
        }
    }

    public enum ServiceStatus
    {
        SUCCESS,
        WARNING,
        DANGER
    }

    public class MostReportedIssue
    {
        public string Issue { get; set; } = string.Empty;
        public int Percentage { get; set; }
    }

    public class ChartPoint
    {
        public DateTime Time { get; set; }
        public int Report { get; set; } // Number of report at this time
        public int Baseline { get; set; } // Expected number of reports at this time
    }
}
