using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }

        [Required]
        public int SpareItemId { get; set; }

        public SpareItem SpareItem { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string AdjustmentType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int PreviousQuantity { get; set; }

        public int NewQuantity { get; set; }

        [StringLength(300)]
        public string? Reason { get; set; }

        [Required]
        public string AdjustedByUserId { get; set; } = string.Empty;

        public DateTime AdjustedAt { get; set; } = DateTime.UtcNow;
    }
}