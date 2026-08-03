namespace CampusStrayCatSystem.Models {
    public class CatPhotoFeatureResponse {
        public string PhotoID { get; set; } = string.Empty;
        public string? CatID { get; set; }
        public double[]? FeatureVector { get; set; }
    }
}
