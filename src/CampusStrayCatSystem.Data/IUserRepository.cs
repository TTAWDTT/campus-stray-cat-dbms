namespace CampusStrayCatSystem.Data
{
    public interface IUserRepository
    {
        Task<CampusStrayCatSystem.Models.User?> GetById(string userId);
        Task<CampusStrayCatSystem.Models.User?> GetByUsername(string username);
        Task<IEnumerable<CampusStrayCatSystem.Models.User>> GetAll(string? username, string? status, string? roleId);
        Task<int> Create(CampusStrayCatSystem.Models.User user);
        Task<int> Update(CampusStrayCatSystem.Models.User user);
        Task<int> UpdateStatus(string userId, string status);
        Task<bool> Exists(string userId);
        Task<bool> UsernameExists(string username);
    }
}
