using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    /// <summary>
    /// 猫咪失踪预警。
    /// 用来追踪某只猫长时间未被目击后的预警、处理和关闭过程。
    /// </summary>
    public class CatMissingAlert
    {
        public string AlertID { get; set; } = string.Empty;
        public string CatID { get; set; } = string.Empty;
        public string? LastSightingID { get; set; }
        public DateTime? LastSightingTime { get; set; }
        [Range(1, 3650, ErrorMessage = "阈值天数必须在 1 到 3650 之间。")]
        public int? ThresholdDays { get; set; }
        public DateTime? AlertTime { get; set; }
        public string AlertStatus { get; set; } = "PROCESSING";
        public string? HandlerUserID { get; set; }
        public DateTime? CloseTime { get; set; }
        public string? Remark { get; set; }
    }
}
