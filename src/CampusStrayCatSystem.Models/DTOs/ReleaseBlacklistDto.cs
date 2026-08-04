using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 解除黑名单请求DTO
    /// </summary>
    public class ReleaseBlacklistDto
    {
        public string? BlacklistId { get; set; }

        [MaxLength(500, ErrorMessage = "解除说明长度不能超过500")]
        public string ReleaseReason { get; set; }  // 可选
    }
}
