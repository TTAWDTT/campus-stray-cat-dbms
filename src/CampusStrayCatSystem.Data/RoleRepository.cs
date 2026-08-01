using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<Role>> GetAll()
        {
            const string sql = @"
                SELECT ROLEID AS RoleID,
                       ROLENAME AS RoleName,
                       DESCRIPTION AS Description,
                       PERMISSIONSCOPE AS PermissionScope
                FROM SYS_ROLES
                ORDER BY ROLENAME";

            return await QueryAsync(sql);
        }

        public async Task<Role?> GetByIdRole(string id)
        {
            const string sql = @"
                SELECT ROLEID AS RoleID,
                       ROLENAME AS RoleName,
                       DESCRIPTION AS Description,
                       PERMISSIONSCOPE AS PermissionScope
                FROM SYS_ROLES
                WHERE ROLEID = :RoleID";

            return await QuerySingleAsync(sql, new { RoleID = id });
        }

        public async Task<int> CreateRole(Role role)
        {
            const string sql = @"
                INSERT INTO SYS_ROLES (ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE)
                VALUES (:RoleID, :RoleName, :Description, :PermissionScope)";

            return await ExecuteAsync(sql, new
            {
                role.RoleID,
                role.RoleName,
                role.Description,
                role.PermissionScope
            });
        }

        public async Task<int> UpdateRole(Role role)
        {
            const string sql = @"
                UPDATE SYS_ROLES
                SET ROLENAME = :RoleName,
                    DESCRIPTION = :Description,
                    PERMISSIONSCOPE = :PermissionScope
                WHERE ROLEID = :RoleID";

            return await ExecuteAsync(sql, new
            {
                role.RoleName,
                role.Description,
                role.PermissionScope,
                role.RoleID
            });
        }

        public async Task<int> DeleteRole(string id)
        {
            const string sql = "DELETE FROM SYS_ROLES WHERE ROLEID = :RoleID";
            return await ExecuteAsync(sql, new { RoleID = id });
        }
    }
}
