using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class NestMaintenanceRecord
    {
        [StringLength(36, ErrorMessage = "维护记录 ID 不能超过 36 个字符。")]
        public string MaintenanceID { get; set; } = string.Empty;

        [StringLength(36, ErrorMessage = "点位 ID 不能超过 36 个字符。")]
        public string? PointID { get; set; }

        [StringLength(50, ErrorMessage = "材料类型不能超过 50 个字符。")]
        public string? MaterialType { get; set; }

        public DateTime? CheckTime { get; set; }

        [StringLength(100, ErrorMessage = "天气情况不能超过 100 个字符。")]
        public string? WeatherCondition { get; set; }

        [StringLength(20, ErrorMessage = "损坏程度不能超过 20 个字符。")]
        public string? DamageLevel { get; set; }

        [StringLength(30, ErrorMessage = "维护动作不能超过 30 个字符。")]
        public string? ActionType { get; set; }

        [StringLength(36, ErrorMessage = "操作用户 ID 不能超过 36 个字符。")]
        public string? OperatorUserID { get; set; }

        public DateTime? NextCheckTime { get; set; }

        [StringLength(300, ErrorMessage = "备注不能超过 300 个字符。")]
        public string? Remark { get; set; }
    }
}
