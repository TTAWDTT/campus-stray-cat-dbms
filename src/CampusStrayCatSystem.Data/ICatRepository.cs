using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data {
    public interface ICatRepository {
        Task<bool> Exists(string catId);
        Task<IEnumerable<CatSummary>> GetAllAsync(string? mainAreaId = null,
                                                  string? lifeStatus = null,
                                                  string? archiveStatus = null);
        Task<CatSummary?> GetByIdAsync(string catId);
    }
}
