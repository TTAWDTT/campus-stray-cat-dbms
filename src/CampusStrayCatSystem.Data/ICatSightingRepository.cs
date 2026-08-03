using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface ICatSightingRepository
    {
        Task<IEnumerable<CatSighting>> GetAllAsync(
            string? catId = null,
            string? areaId = null,
            DateTime? from = null,
            DateTime? to = null);

        Task<CatSighting?> GetByIdAsync(string id);
        Task<IEnumerable<CatSighting>> GetRecentByCatAsync(string catId, int limit);
        Task<bool> CatExistsAsync(string catId);
        Task<bool> UserExistsAsync(string userId);
        Task<bool> HasReferencesAsync(string id);
        Task<int> CreateAsync(CatSighting sighting);
        Task<int> UpdateAsync(CatSighting sighting);
        Task<int> DeleteAsync(string id);
    }
}
