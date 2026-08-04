using System.Collections.Generic;
using System.Threading.Tasks;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAll();
        Task<Role?> GetByIdRole(string id);
        Task<int> CreateRole(Role role);
        Task<int> UpdateRole(Role role);
        Task<int> DeleteRole(string id);
        Task<int> AssignRole(string userId, string roleId);
        Task<int> GetUserCount(string roleId);
    }
}
