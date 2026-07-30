namespace CampusStrayCatSystem.Models
{
    public static class TnrStatuses
    {
        public const string Discovered = "DISCOVERED";
        public const string Captured = "CAPTURED";
        public const string Surgery = "SURGERY";
        public const string Recovering = "RECOVERING";
        public const string Released = "RELEASED";
        public const string Cancelled = "CANCELLED";

        public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            Discovered, Captured, Surgery, Recovering, Released, Cancelled
        };

        public static bool IsValid(string? status)
            => status != null && Allowed.Contains(status);
    }

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
