using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public enum AnnouncementType
    {
        General,
        Safety,
        Priority
    }

    public class ShopAnnouncement
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public AnnouncementType Type { get; set; } = AnnouncementType.General;

        public bool IsPinned { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string AuthorName { get; set; } = string.Empty;
    }
}
