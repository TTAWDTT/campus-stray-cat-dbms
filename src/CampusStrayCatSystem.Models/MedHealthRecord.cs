namespace CampusStrayCatSystem.Models
{
    public class MedHealthRecord
    {
        public string RecordID { get; set; } = string.Empty;
        public string CatID { get; set; } = string.Empty;
        public string? RecordType { get; set; }
        public string? HospitalName { get; set; }
        public string? Diagnosis { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
