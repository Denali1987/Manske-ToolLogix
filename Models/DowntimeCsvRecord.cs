using CsvHelper;

namespace ToolLogix.Models
{
    [CsvRecord]
    public partial class DowntimeCsvRecord
    {
        public string WorkCenter { get; set; } = string.Empty;

        public string? WorkOrder { get; set; }

        public string? PartNumber { get; set; }

        public string? Tool { get; set; }

        public string DowntimeReason { get; set; } = string.Empty;

        public string DowntimeReasonCode { get; set; } = string.Empty;

        [CsvColumn("Duration_min_")]
        public double DurationMinutes { get; set; }

        public DateTime LocalStartTime { get; set; }

        public DateTime LocalEndTime { get; set; }

        public int Planned { get; set; }

        public bool Costed { get; set; }

        public string? SAPBucket { get; set; }

        public string EmployeeName { get; set; } = string.Empty;
    }
}