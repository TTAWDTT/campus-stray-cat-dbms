using System;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 角色响应DTO（用于列表和详情返回）
    /// </summary>
    public class RoleResponseDto
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string PermissionScope { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IsActive { get; set; }
        public int UserCount { get; set; }  // 拥有该角色的用户数
    }
}