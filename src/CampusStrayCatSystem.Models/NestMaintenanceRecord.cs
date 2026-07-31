namespace CampusStrayCatSystem.Models
{
    public class NestMaintenanceRecord
    {
        public string MaintenanceID { get; set; } = string.Empty;
        public string? PointID { get; set; }
        public string? MaterialType { get; set; }
        public DateTime? CheckTime { get; set; }
        public string? WeatherCondition { get; set; }
        public string? DamageLevel { get; set; }
        public string? ActionType { get; set; }
        public string? OperatorUserID { get; set; }
        public DateTime? NextCheckTime { get; set; }
        public string? Remark { get; set; }
    }
}
