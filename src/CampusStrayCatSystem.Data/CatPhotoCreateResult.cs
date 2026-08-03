using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data {
    public class CatPhotoCreateResult {
        public CatPhotoMutationStatus Status { get; set; }
        public CatPhoto? Photo { get; set; }
    }
}
