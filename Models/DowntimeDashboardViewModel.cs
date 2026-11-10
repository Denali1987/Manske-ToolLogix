using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class DowntimeDashboardViewModel
    {
        [Display(Name = "Downtime CSV")]
        public IFormFile? CsvFile { get; set; }

        public string? FileName { get; set; }

        public DateTime? AsOfTime { get; set; }

        public int RepairInProcessCount { get; set; }

        public int ChangeoverInProcessCount { get; set; }

        public int WaitingForRepairCount { get; set; }

        public int WaitingForChangeoverCount { get; set; }

        public List<DowntimeRowViewModel> InProcess { get; set; } = new();

        public List<DowntimeRowViewModel> Waiting { get; set; } = new();

        public List<string> Warnings { get; set; } = new();
    }

    public class DowntimeRowViewModel
    {
        public string WorkCenter { get; set; } = string.Empty;

        public string? WorkOrder { get; set; }

        public string? PartNumber { get; set; }

        public string? Tool { get; set; }

        public string DowntimeReason { get; set; } = string.Empty;

        public string DowntimeReasonCode { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public double DurationMinutes { get; set; }

        public bool Planned { get; set; }

        public bool Costed { get; set; }

        public string? SapBucket { get; set; }

        public string ElapsedDisplay
        {
            get
            {
                var elapsed = TimeSpan.FromMinutes(DurationMinutes);

                if (elapsed.TotalHours >= 1)
                {
                    return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
                }

                return $"{elapsed.Minutes}m";
            }
        }
    }
}