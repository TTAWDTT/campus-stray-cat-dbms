using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface ICatRepository
    {
        Task<bool> Exists(string catId);
    }
}
