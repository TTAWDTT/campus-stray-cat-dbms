using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data {
    public interface ICatMatchRepository {
        Task<IEnumerable<CatMatchRecord>> GetBySourcePhotoAsync(string catID,
                                                                string photoID,
                                                                string? candidateCatID,
                                                                string? confirmStatus);
        Task<CatMatchRecord?> GetByIDAsync(string matchID);
        Task<CatMatchMutationStatus> ConfirmAsync(string matchID,
                                                  string confirmStatus,
                                                  string confirmUserID);
    }
}
