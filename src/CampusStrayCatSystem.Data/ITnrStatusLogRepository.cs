using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface ITnrStatusLogRepository
    {
        Task<IEnumerable<TnrStatusLog>> GetByCaseId(string caseId);
        Task<int> Create(TnrStatusLog log);
    }
}
