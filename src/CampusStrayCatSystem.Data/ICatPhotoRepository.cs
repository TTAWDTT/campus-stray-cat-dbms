using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data {
    public interface ICatPhotoRepository {
        Task<IEnumerable<CatPhoto>> GetByCatIDAsync(string catID);
        Task<CatPhoto?> GetByIDAsync(string catID, string photoID);
        Task<CatPhotoFeatureData?> GetFeatureAsync(string catID, string photoID);
        Task<CatPhotoCreateResult> CreateAsync(CatPhoto photo, int requestedPrimary);
        Task<CatPhotoMutationStatus> SetPrimaryAsync(string catID, string photoID);
        Task<CatPhotoMutationStatus> DeleteAsync(string catID, string photoID);
    }
}
