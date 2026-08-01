using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class UpdateUserStatusRequest
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}
