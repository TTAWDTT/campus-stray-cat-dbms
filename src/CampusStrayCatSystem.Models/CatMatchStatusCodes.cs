namespace CampusStrayCatSystem.Models {
    public static class CatMatchStatusCodes {
        public const string Pending = "PENDING";
        public const string Confirmed = "CONFIRMED";
        public const string Rejected = "REJECTED";

        public static string Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? Pending : value.Trim().ToUpperInvariant();

        public static bool IsKnown(string? value) => Normalize(value) is Pending or Confirmed or Rejected;

        public static bool IsDecision(string? value) => Normalize(value) is Confirmed or Rejected;
    }
}
