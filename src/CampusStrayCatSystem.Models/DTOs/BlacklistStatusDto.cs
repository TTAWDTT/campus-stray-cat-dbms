using System;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 用户黑名单状态响应DTO（供领养审核模块调用）
    /// </summary>
    public class BlacklistStatusDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 是否在黑名单中
        /// </summary>
        public bool IsBlacklisted { get; set; }

        /// <summary>
        /// 黑名单记录ID（如果在黑名单中）
        /// </summary>
        public string BlacklistId { get; set; }

        /// <summary>
        /// 拉黑原因类型
        /// </summary>
        public string ReasonType { get; set; }

        /// <summary>
        /// 拉黑详细原因
        /// </summary>
        public string ReasonDetail { get; set; }

        /// <summary>
        /// 拉黑时间
        /// </summary>
        public DateTime? BlacklistedAt { get; set; }
    }
}