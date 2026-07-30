namespace CampusStrayCatSystem.Models
{
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
