using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 给用户分配角色请求DTO
    /// </summary>
    public class AssignRoleDto
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "角色ID不能为空")]
        public string RoleId { get; set; }
    }
}