using System;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 黑名单列表响应DTO
    /// </summary>
    public class BlacklistResponseDto
    {
        public string BlacklistId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }  // 关联查询用户姓名
        public string ReasonType { get; set; }
        public string ReasonDetail { get; set; }
        public string ApplicationId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }  // 操作人姓名
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public DateTime? ReleaseTime { get; set; }
        public string ReleasedBy { get; set; }
        public string ReleasedByName { get; set; }
    }
}