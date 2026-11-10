using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToolLogix.Models
{
    public class SpareItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter a detail number.")]
        [Display(Name = "Detail Number")]
        [StringLength(100)]
        public string DetailNumber { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Display(Name = "Asset")]
        public int? AssetId { get; set; }

        public Asset? Asset { get; set; }

        [Display(Name = "Storage Location")]
        [StringLength(100)]
        public string? StorageLocation { get; set; }

        [Display(Name = "Quantity on Hand")]
        [Range(0, 100000)]
        public int QuantityOnHand { get; set; }

        [Display(Name = "Minimum Quantity")]
        [Range(0, 100000)]
        public int MinimumQuantity { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string StockStatus
        {
            get
            {
                if (QuantityOnHand == 0)
                {
                    return "Out of Stock";
                }

                if (QuantityOnHand <= MinimumQuantity)
                {
                    return "Low Stock";
                }

                return "In Stock";
            }
        }
    }
}