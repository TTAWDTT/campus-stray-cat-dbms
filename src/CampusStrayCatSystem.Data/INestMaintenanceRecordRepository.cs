using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface INestMaintenanceRecordRepository
    {
        Task<IEnumerable<NestMaintenanceRecord>> GetAllAsync(
            string? pointId = null,
            string? damageLevel = null,
            DateTime? from = null,
            DateTime? to = null);

        Task<NestMaintenanceRecord?> GetByIdAsync(string id);
        Task<bool> UserExistsAsync(string userId);
        Task<int> CreateAsync(NestMaintenanceRecord record);
        Task<int> UpdateAsync(NestMaintenanceRecord record);
        Task<int> DeleteAsync(string id);
    }
}
