using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class SpareInventoryIndexViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Asset")]
        public int? AssetId { get; set; }

        [Display(Name = "Stock Status")]
        public string? StockStatus { get; set; }

        public List<SelectListItem> AssetOptions { get; set; } = new();

        public List<SpareInventoryRowViewModel> SpareItems { get; set; } = new();

        public int TotalItemCount { get; set; }

        public int LowStockCount { get; set; }

        public int OutOfStockCount { get; set; }

        public int TotalQuantityOnHand { get; set; }
    }

    public class SpareInventoryRowViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Detail Number")]
        public string DetailNumber { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? AssetId { get; set; }

        [Display(Name = "Asset")]
        public string? AssetNumber { get; set; }

        [Display(Name = "Storage Location")]
        public string? StorageLocation { get; set; }

        [Display(Name = "Quantity on Hand")]
        public int QuantityOnHand { get; set; }

        [Display(Name = "Minimum Quantity")]
        public int MinimumQuantity { get; set; }

        [Display(Name = "Stock Status")]
        public string StockStatus { get; set; } = string.Empty;

        [Display(Name = "Last Updated")]
        public DateTime LastUpdatedAt { get; set; }
    }
}