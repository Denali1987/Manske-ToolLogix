using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class WorkLog
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Work Date")]
        [DataType(DataType.Date)]
        public DateTime WorkDate { get; set; } = DateTime.Today;

        [Required]
        public int AssetId { get; set; }

        public Asset Asset { get; set; } = null!;

        [Display(Name = "Work Order Number")]
        [StringLength(50)]
        public string? WorkOrderNumber { get; set; }

        [Required]
        [Display(Name = "Issue")]
        [StringLength(500)]
        public string Issue { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Work Done")]
        [StringLength(500)]
        public string WorkDone { get; set; } = string.Empty;

        [Display(Name = "Additional Notes")]
        [StringLength(4000)]
        public string? Notes { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LaborEntry> LaborEntries { get; set; }
            = new List<LaborEntry>();

        public ICollection<DetailUsage> DetailsUsed { get; set; }
            = new List<DetailUsage>();
    }
}