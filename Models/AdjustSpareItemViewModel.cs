using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class AdjustSpareItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Detail Number")]
        public string DetailNumber { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Display(Name = "Current Quantity")]
        public int CurrentQuantity { get; set; }

        [Required(ErrorMessage = "Select an adjustment type.")]
        [Display(Name = "Adjustment Type")]
        public string AdjustmentType { get; set; } = "Add";

        [Display(Name = "Quantity")]
        [Range(0, 100000)]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Enter a reason for the adjustment.")]
        [Display(Name = "Reason")]
        [StringLength(300)]
        public string Reason { get; set; } = string.Empty;
    }
}