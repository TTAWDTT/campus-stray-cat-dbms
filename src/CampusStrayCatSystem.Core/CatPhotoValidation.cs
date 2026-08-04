using System.Text;

namespace CampusStrayCatSystem.Core {
    public static class CatPhotoValidation {
        public static bool IsSafePathIdentifier(string? value) {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 36) {
                return false;}

            return value.All(character =>
                character is >= 'a' and <= 'z' or
                             >= 'A' and <= 'Z' or
                             >= '0' and <= '9' or '-' or '_');}

        public static bool IsValidDatabaseID(string? value) =>
            !string.IsNullOrWhiteSpace(value) && Encoding.UTF8.GetByteCount(value.Trim()) <= 36;
    }
}
