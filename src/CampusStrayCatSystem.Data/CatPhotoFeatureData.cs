namespace CampusStrayCatSystem.Data {
    public class CatPhotoFeatureData {
        public string PhotoID { get; set; } = string.Empty;
        public string? CatID { get; set; }
        public string? FeatureVectorJson { get; set; }
    }
}
