using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data {
    public interface ICampusAreaRepository {
        Task<IEnumerable<CampusArea>> GetAllAsync();
        Task<CampusArea?> GetByIdAsync(string areaId);
    }
}
