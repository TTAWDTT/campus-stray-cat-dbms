using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface ICampusAreaRepository
    {
        Task<IEnumerable<CampusArea>> GetAllAsync(
            string? campusName = null,
            string? areaType = null,
            string? riskLevel = null);

        Task<CampusArea?> GetByIdAsync(string id);
        Task<IEnumerable<CampusArea>> GetRootsAsync();
        Task<IEnumerable<CampusArea>> GetChildrenAsync(string parentAreaId);
        Task<IEnumerable<CampusAreaHierarchyItem>> GetHierarchyAsync();
        Task<bool> HasReferencesAsync(string id);
        Task<int> CreateAsync(CampusArea area);
        Task<int> UpdateAsync(CampusArea area);
        Task<int> DeleteAsync(string id);
    }
}
