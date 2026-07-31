namespace CampusStrayCatSystem.Models
{
    public class ServicePoint
    {
        public string PointID { get; set; } = string.Empty;
        public string? AreaID { get; set; }
        public string PointName { get; set; } = string.Empty;
        public string? PointType { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public string? FacilityStatus { get; set; }
        public DateTime? DeployTime { get; set; }
    }
}
