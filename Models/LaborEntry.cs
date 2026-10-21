using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ToolLogix.Models
{
    public class LaborEntry
    {
        public int Id { get; set; }

        [Required]
        public int WorkLogId { get; set; }

        public WorkLog WorkLog { get; set; } = null!;

        [Required]
        [Display(Name = "Employee")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Hours")]
        [Range(typeof(decimal), "0.01", "24.00",
            ErrorMessage = "Hours must be between 0.01 and 24.")]
        [Precision(5, 2)]
        public decimal Hours { get; set; }
    }
}