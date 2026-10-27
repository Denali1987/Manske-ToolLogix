namespace ToolLogix.Models
{
    public class AssetDetailsViewModel
    {
        public Asset Asset { get; set; } = null!;

        public string? PrintUrl { get; set; }

        public List<WorkLogRowViewModel> RecentWorkLogs { get; set; }
            = new();

        public int TotalWorkLogCount { get; set; }
    }
}