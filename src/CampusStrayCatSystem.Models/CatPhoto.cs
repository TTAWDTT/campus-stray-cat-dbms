namespace CampusStrayCatSystem.Models {
    public class CatPhoto {
        private DateTime? _uploadTime;

        public string PhotoID { get; set; } = string.Empty;
        public string? CatID { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public string? UploadUserID { get; set; }
        public DateTime? UploadTime {
            get => _uploadTime;
            set => _uploadTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
        }
        public int IsPrimary { get; set; }
    }
}
