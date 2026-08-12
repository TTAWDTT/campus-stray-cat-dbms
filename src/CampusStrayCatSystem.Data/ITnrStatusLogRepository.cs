using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface ITnrStatusLogRepository
    {
        Task<IEnumerable<TnrStatusLog>> GetByCaseId(string caseId);
        Task<TnrStatusLog?> GetById(string logId);
        Task<int> Create(TnrStatusLog log);
    }
}
