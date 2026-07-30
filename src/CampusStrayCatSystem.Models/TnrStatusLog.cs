namespace CampusStrayCatSystem.Models
{
    public class TnrStatusLog
    {
        public string LogID { get; set; } = string.Empty;
        public string CaseID { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
        public string? OperatorID { get; set; }
        public DateTime? OpTime { get; set; }
        public string? Remark { get; set; }
    }
}
