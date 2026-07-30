namespace CampusStrayCatSystem.Models
{
    public static class MedRecordTypes
    {
        public const string Vaccination = "VACCINATION";
        public const string Checkup = "CHECKUP";
        public const string Treatment = "TREATMENT";
        public const string Surgery = "SURGERY";
        public const string Deworming = "DEWORMING";
        public const string Emergency = "EMERGENCY";
        public const string Other = "OTHER";

        public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            Vaccination, Checkup, Treatment, Surgery, Deworming, Emergency, Other
        };

        public static bool IsValid(string? recordType)
            => recordType != null && Allowed.Contains(recordType);
    }

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
