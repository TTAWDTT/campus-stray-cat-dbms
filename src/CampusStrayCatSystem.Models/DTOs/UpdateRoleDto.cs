using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 修改角色请求DTO
    /// </summary>
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "角色名称不能为空")]
        [MaxLength(50, ErrorMessage = "角色名称长度不能超过50")]
        public string RoleName { get; set; }

        [MaxLength(500, ErrorMessage = "权限范围描述长度不能超过500")]
        public string PermissionScope { get; set; }

        [MaxLength(200, ErrorMessage = "角色描述长度不能超过200")]
        public string Description { get; set; }

        [Required(ErrorMessage = "是否启用不能为空")]
        public string IsActive { get; set; }  // "1" 或 "0"
    }
}