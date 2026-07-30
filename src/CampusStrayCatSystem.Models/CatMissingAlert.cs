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
        public int? ThresholdDays { get; set; }
        public DateTime? AlertTime { get; set; }
        public string AlertStatus { get; set; } = "PROCESSING";
        public string? HandlerUserID { get; set; }
        public DateTime? CloseTime { get; set; }
        public string? Remark { get; set; }
    }
}