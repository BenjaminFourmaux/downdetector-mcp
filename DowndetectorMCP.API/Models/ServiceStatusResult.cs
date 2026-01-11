using System;
using System.Collections.Generic;
using System.Text;
using DowndetectorMCP.API.Utils;

namespace DowndetectorMCP.API.Models
{
    public class ServiceStatusResult
    {
        public string ServiceName { get; set; } = string.Empty;
        public ServiceStatus Status { get; set; } = ServiceStatus.SUCCESS;
        public Dictionary<string, int> MostReportedIssues { get; set; } = new();
        public List<ChartPoint> ChartData { get; set; } = new();
        public ChartPoint LastReportData => ChartData.Count > 0 ? ChartData.Last() : new ChartPoint();

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

    public class ChartPoint
    {
        public DateTime Time { get; set; }
        public int Reports { get; set; } // Number of reports at this time
        public int Baseline { get; set; } // Expected number of reports at this time
    }
}
