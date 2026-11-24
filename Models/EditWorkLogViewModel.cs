using System.ComponentModel.DataAnnotations;

namespace ToolLogix.Models
{
    public class EditWorkLogViewModel :
        CreateWorkLogViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}