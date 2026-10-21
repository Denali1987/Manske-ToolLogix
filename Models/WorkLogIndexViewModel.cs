using Microsoft.AspNetCore.Mvc.Rendering;

namespace ToolLogix.Models
{
    public class WorkLogIndexViewModel
    {
        public string? SearchTerm { get; set; }

        public int? AssetId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<WorkLogRowViewModel> WorkLogs { get; set; }
            = new();

        public IEnumerable<SelectListItem> AssetOptions { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }

    public class WorkLogRowViewModel
    {
        public int Id { get; set; }

        public DateTime WorkDate { get; set; }

        public string AssetNumber { get; set; } = string.Empty;

        public string? ProductNumber { get; set; }

        public string? WorkOrderNumber { get; set; }

        public string Issue { get; set; } = string.Empty;

        public string WorkDone { get; set; } = string.Empty;

        public string Employees { get; set; } = string.Empty;

        public string DetailsUsed { get; set; } = string.Empty;

        public decimal TotalHours { get; set; }
    }
}