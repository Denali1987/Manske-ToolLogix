using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ToolLogix.Models
{
    [Index(nameof(AssetNumber), IsUnique = true)]
    public class Asset
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Asset Number")]
        public string AssetNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Press")]
        public string? PressNumber { get; set; }

        [StringLength(50)]
        public string? Location { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}