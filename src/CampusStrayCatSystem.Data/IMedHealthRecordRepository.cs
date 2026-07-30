using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface IMedHealthRecordRepository
    {
        Task<IEnumerable<MedHealthRecord>> GetAll();
        Task<IEnumerable<MedHealthRecord>> GetByCatId(string catId);
        Task<MedHealthRecord?> GetById(string id);
        Task<int> Create(MedHealthRecord record);
        Task<int> Update(MedHealthRecord record);
        Task<int> Delete(string id);
    }
}
