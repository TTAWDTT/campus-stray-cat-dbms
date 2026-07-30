namespace CampusStrayCatSystem.Models {
    public class CampusArea {
        public string AreaId { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string? CampusName { get; set; }
        public string? ParentAreaId { get; set; }
        public string? AreaType { get; set; }
        public string? RiskLevel { get; set; }
        public string? GeoBoundary { get; set; }
    }
}
