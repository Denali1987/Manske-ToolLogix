namespace ToolLogix.Models
{
    public class AssetDetailsViewModel
    {
        public Asset Asset { get; set; } = null!;

        public string? PrintUrl { get; set; }

        public List<WorkLogRowViewModel> RecentWorkLogs { get; set; }
            = new();

        public int TotalWorkLogCount { get; set; }

        public int LaborDays { get; set; } = 1;

        public decimal LaborTotalHours { get; set; }

        public List<AssetLaborChargeViewModel> LaborCharges { get; set; }
            = new();

        public List<AssetDetailUsageViewModel> RecentDetailUsages { get; set; }
            = new();
    }

    public class AssetLaborChargeViewModel
    {
        public DateTime WorkDate { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string? WorkOrderNumber { get; set; }

        public decimal Hours { get; set; }
    }

    public class AssetDetailUsageViewModel
    {
        public DateTime WorkDate { get; set; }

        public string DetailNumber { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Quantity { get; set; }

        public string UsedByName { get; set; } = string.Empty;
    }
}