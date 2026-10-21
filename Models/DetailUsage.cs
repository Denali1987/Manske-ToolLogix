using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class DetailUsage
    {
        public int Id { get; set; }

        [Required]
        public int WorkLogId { get; set; }

        public WorkLog WorkLog { get; set; } = null!;

        [Required]
        [Display(Name = "Detail Number")]
        [StringLength(100)]
        public string DetailNumber { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;
    }
}