using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface ITnrCaseRepository
    {
        Task<IEnumerable<TnrCase>> GetAll();
        Task<TnrCase?> GetById(string id);
        Task<int> Create(TnrCase tnrCase);
        Task<int> Update(TnrCase tnrCase);
        Task<int> Delete(string id);
    }
}
