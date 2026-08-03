namespace CampusStrayCatSystem.Core {
    public class CatPhotoServiceResult<T> {
        public CatPhotoServiceStatus Status { get; set; }
        public T? Value { get; set; }
    }
}
