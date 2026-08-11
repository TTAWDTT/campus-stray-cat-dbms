using System.Collections.Generic;
using System.Threading.Tasks;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAll();
        Task<Role?> GetByIdRole(string id);
        Task<bool> ExistsByNameAsync(string roleName);  // ✅ 新增
        Task<bool> ExistsByIdAsync(string roleId);      // ✅ 新增
        Task<int> CreateRole(Role role);
        Task<int> UpdateRole(Role role);
        Task<int> DeleteRole(string id);
        Task<int> AssignRole(string userId, string roleId);
        Task<string> AssignRoleWithAuditAsync(string userId, string newRoleId, string operatorId);  // ✅ 新增
        Task<int> CreateRoleWithAuditAsync(Role role, string operatorId);
        Task<int> GetUserCount(string roleId);
    }
}
