using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ToolLogix.Models
{
    public class CreateWorkLogViewModel
    {
        [Required]
        [Display(Name = "Work Date")]
        [DataType(DataType.Date)]
        public DateTime WorkDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Select an asset.")]
        [Display(Name = "Asset Number")]
        public int? AssetId { get; set; }

        [Display(Name = "Work Order Number")]
        [StringLength(50)]
        public string? WorkOrderNumber { get; set; }

        [Required(ErrorMessage = "Enter the issue that required work.")]
        [Display(Name = "Issue")]
        [StringLength(500)]
        public string Issue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Describe the work performed.")]
        [Display(Name = "Work Done")]
        [StringLength(500)]
        public string WorkDone { get; set; } = string.Empty;

        [Display(Name = "Additional Notes")]
        [StringLength(4000)]
        public string? Notes { get; set; }

        public List<LaborEntryInputModel> LaborEntries { get; set; }
            = new() { new LaborEntryInputModel() };

        public List<DetailUsageInputModel> DetailsUsed { get; set; }
            = new() { new DetailUsageInputModel() };

        public IEnumerable<SelectListItem> AssetOptions { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> EmployeeOptions { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }

    public class LaborEntryInputModel
    {
        [Display(Name = "Employee")]
        public string? UserId { get; set; }

        [Display(Name = "Hours")]
        [Range(typeof(decimal), "0.25", "16.00",
            ErrorMessage = "Hours must be between 0.25 and 16.")]
        public decimal? Hours { get; set; }
    }

    public class DetailUsageInputModel
    {
        [Display(Name = "Detail Number")]
        [StringLength(100)]
        public string? DetailNumber { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;
    }
}