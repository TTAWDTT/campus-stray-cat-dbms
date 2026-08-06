using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    /// <summary>
    /// 紧急救助上报。
    /// 保存用户发现伤猫、困猫或疑似病猫时提交的救助信息。
    /// </summary>
    public class EmergencyReport
    {
        public string ReportID { get; set; } = string.Empty;
        public string ReporterUserID { get; set; } = string.Empty;
        public string AreaID { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string? PhotoURL { get; set; }
        [Range(-180, 180, ErrorMessage = "经度必须在 -180 到 180 之间。")]
        public decimal? Longitude { get; set; }
        [Range(-90, 90, ErrorMessage = "纬度必须在 -90 到 90 之间。")]
        public decimal? Latitude { get; set; }
        public DateTime? ReportTime { get; set; }
        public string UrgencyLevel { get; set; } = "LOW";
        public string ProcessStatus { get; set; } = "SUBMITTED";
        public string? HandlerUserID { get; set; }
        public string? ProcessResult { get; set; }
    }
}
