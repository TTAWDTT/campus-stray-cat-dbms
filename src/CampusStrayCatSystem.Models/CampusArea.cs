namespace CampusStrayCatSystem.Models
{
    public class CampusArea
    {
        public string AreaID { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string? CampusName { get; set; }
        public string? ParentAreaID { get; set; }
        public string? AreaType { get; set; }
        public string? RiskLevel { get; set; }
        public string? GeoBoundary { get; set; }
    }
}
