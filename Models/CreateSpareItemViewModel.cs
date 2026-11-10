using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class CreateSpareItemViewModel
    {
        [Required(ErrorMessage = "Enter a detail number.")]
        [Display(Name = "Detail Number")]
        [StringLength(100)]
        public string DetailNumber { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Display(Name = "Asset")]
        public int? AssetId { get; set; }

        [Display(Name = "Storage Location")]
        [StringLength(100)]
        public string? StorageLocation { get; set; }

        [Display(Name = "Starting Quantity")]
        [Range(0, 100000)]
        public int QuantityOnHand { get; set; }

        [Display(Name = "Minimum Quantity")]
        [Range(0, 100000)]
        public int MinimumQuantity { get; set; }

        public List<SelectListItem> AssetOptions { get; set; } = new();
    }
}