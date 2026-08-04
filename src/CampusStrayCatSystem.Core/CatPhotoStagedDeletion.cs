namespace CampusStrayCatSystem.Core {
    public class CatPhotoStagedDeletion {
        public bool FileExisted { get; set; }
        public string OriginalPath { get; set; } = string.Empty;
        public string? StagedPath { get; set; }
    }
}
