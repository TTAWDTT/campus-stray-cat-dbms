using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class UpdateUserRequest
    {
        [Required]
        [StringLength(36)]
        public string RoleID { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RealName { get; set; }

        [StringLength(30)]
        public string? StudentNo { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? VerifyStatus { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }
    }
}
