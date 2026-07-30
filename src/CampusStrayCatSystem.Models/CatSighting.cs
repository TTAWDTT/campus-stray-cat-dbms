namespace CampusStrayCatSystem.Models
{
    /// <summary>
    /// 猫咪最后目击记录。
    /// 失踪预警会引用这条记录，方便把“最后一次看见它”的信息保留下来。
    /// </summary>
    public class CatSighting
    {
        public string SightingID { get; set; } = string.Empty;
        public string CatID { get; set; } = string.Empty;
        public string? UserID { get; set; }
        public string? AreaID { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public string? PhotoURL { get; set; }
        public DateTime? SightingTime { get; set; }
        public string? Remark { get; set; }
    }
}