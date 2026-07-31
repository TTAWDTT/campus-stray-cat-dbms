namespace CampusStrayCatSystem.Models
{
    /// <summary>
    /// 校园猫咪目击记录，可作为失踪预警的最后目击信息。
    /// </summary>
    public class CatSighting
    {
        public string SightingID { get; set; } = string.Empty;
        public string? CatID { get; set; }
        public string? UserID { get; set; }
        public string? AreaID { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime? SightingTime { get; set; }
        public string? Remark { get; set; }
    }
}
