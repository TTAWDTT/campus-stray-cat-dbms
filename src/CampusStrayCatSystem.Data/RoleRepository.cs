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
                SELECT ROLE_ID AS RoleID,
                       ROLE_NAME AS RoleName,
                       DESCRIPTION AS Description,
                       PERMISSIONSCOPE AS PermissionScope
                FROM ROLES
                ORDER BY CREATED_DATE DESC";

            return await QueryAsync(sql);
        }

        public async Task<Role?> GetByIdRole(int id)
        {
            const string sql = @"
                SELECT ROLE_ID AS RoleID,
                       ROLE_NAME AS RoleName,
                       DESCRIPTION AS Description,
                       PERMISSIONSCOPE AS PermissionScope
                FROM ROLES
                WHERE ROLE_ID = :RoleID";

            return await QuerySingleAsync(sql, new { RoleID = id });
        }

        public async Task<int> CreateRole(Role role)
        {
            const string sql = @"
                INSERT INTO ROLES (ROLE_ID, ROLE_NAME, DESCRIPTION, PERMISSIONSCOPE)
                VALUES (ROLES_SEQ.NEXTVAL, :RoleName, :Description, :PermissionScope)";

            return await ExecuteAsync(sql, new
            {
                role.RoleName,
                role.Description,
                role.PermissionScope
            });
        }

        public async Task<int> UpdateRole(Role role)
        {
            const string sql = @"
                UPDATE ROLES
                SET ROLE_NAME = :RoleName,
                    DESCRIPTION = :Description,
                    PERMISSIONSCOPE = :PermissionScope
                WHERE ROLE_ID = :RoleID";

            return await ExecuteAsync(sql, new
            {
                role.RoleName,
                role.Description,
                role.PermissionScope,
                role.RoleID
            });
        }

        public async Task<int> DeleteRole(int id)
        {
            const string sql = @"
                UPDATE ROLES
                SET IS_ACTIVE = 0
                WHERE ROLE_ID = :RoleID";

            return await ExecuteAsync(sql, new { RoleID = id });
        }
    }
}
