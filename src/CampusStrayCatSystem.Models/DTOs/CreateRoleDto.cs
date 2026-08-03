using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 新增角色请求DTO
    /// </summary>
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "角色名称不能为空")]
        [MaxLength(50, ErrorMessage = "角色名称长度不能超过50")]
        public string RoleName { get; set; }

        [MaxLength(500, ErrorMessage = "权限范围描述长度不能超过500")]
        public string PermissionScope { get; set; }

        [MaxLength(200, ErrorMessage = "角色描述长度不能超过200")]
        public string Description { get; set; }
    }
}