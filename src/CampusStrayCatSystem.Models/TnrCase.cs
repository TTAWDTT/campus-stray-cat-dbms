namespace CampusStrayCatSystem.Models
{
    public class TnrCase
    {
        public string CaseID { get; set; } = string.Empty;
        public string CatID { get; set; } = string.Empty;
        public string? ResponsibleUserID { get; set; }
        public string? CurrentStatus { get; set; }
        public string? HospitalName { get; set; }
        public DateTime? CaptureTime { get; set; }
        public DateTime? SurgeryTime { get; set; }
        public DateTime? ReleaseTime { get; set; }
        public decimal? TotalCost { get; set; }
    }
}
