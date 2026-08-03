using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface IServicePointRepository
    {
        Task<IEnumerable<ServicePoint>> GetAllAsync(
            string? areaId = null,
            string? pointType = null,
            string? facilityStatus = null);

        Task<ServicePoint?> GetByIdAsync(string id);
        Task<bool> HasReferencesAsync(string id);
        Task<int> CreateAsync(ServicePoint point);
        Task<int> UpdateAsync(ServicePoint point);
        Task<int> DeleteAsync(string id);
    }
}
