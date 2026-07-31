using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    /// <summary>
    /// 校园猫咪目击记录，可作为失踪预警的最后目击信息。
    /// </summary>
    public class CatSighting
    {
        [StringLength(36, ErrorMessage = "目击记录 ID 不能超过 36 个字符。")]
        public string SightingID { get; set; } = string.Empty;

        [StringLength(36, ErrorMessage = "猫咪 ID 不能超过 36 个字符。")]
        public string? CatID { get; set; }

        [StringLength(36, ErrorMessage = "用户 ID 不能超过 36 个字符。")]
        public string? UserID { get; set; }

        [StringLength(36, ErrorMessage = "区域 ID 不能超过 36 个字符。")]
        public string? AreaID { get; set; }

        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }

        [StringLength(300, ErrorMessage = "照片地址不能超过 300 个字符。")]
        public string? PhotoUrl { get; set; }

        public DateTime? SightingTime { get; set; }

        [StringLength(300, ErrorMessage = "备注不能超过 300 个字符。")]
        public string? Remark { get; set; }
    }
}
