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

        public async Task<int> AssignRole(string userId, string roleId)
        {
            const string sql = @"
                UPDATE SYS_USERS
                SET ROLEID = :RoleID
                WHERE USERID = :UserID";

            return await ExecuteAsync(sql, new { UserID = userId, RoleID = roleId });
        }

        public async Task<int> GetUserCount(string roleId)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_USERS WHERE ROLEID = :RoleID";
            return await QuerySingleAsync<int>(sql, new { RoleID = roleId });
        }
        public async Task<bool> ExistsByNameAsync(string roleName)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_ROLES WHERE UPPER(ROLENAME) = UPPER(:RoleName)";
            var count = await QuerySingleAsync<int>(sql, new { RoleName = roleName });
            return count > 0;
        }

        public async Task<bool> ExistsByIdAsync(string roleId)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_ROLES WHERE ROLEID = :RoleId";
            var count = await QuerySingleAsync<int>(sql, new { RoleId = roleId });
            return count > 0;
        }

        // ✅ 修改 CreateRole 增加审计日志（调用存储过程或手动添加）
        public async Task<int> CreateRoleWithAuditAsync(Role role, string operatorId)
        {
            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();
            try {
                // 1. 插入角色
                const string insertSql = @"
                    INSERT INTO SYS_ROLES (ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE)
                    VALUES (:RoleID, :RoleName, :Description, :PermissionScope)";
        
                var rows = await conn.ExecuteAsync(insertSql, new
                {
                    role.RoleID,
                    role.RoleName,
                    role.Description,
                    role.PermissionScope
                }, transaction);
        
                // 2. 写入审计日志
                const string auditSql = @"
                    INSERT INTO LOG_AUDITTRAILS (
                        LOGID, TABLENAME, RECORDID, ACTIONTYPE, NEWVALUE, OPERATORID, OPTIME
                    ) VALUES (
                        :LogId, 'SYS_ROLES', :RecordId, 'INSERT_ROLE', 
                        :NewValue, :OperatorId, SYSTIMESTAMP
                    )";
        
                await conn.ExecuteAsync(auditSql, new
                {
                    LogId = Guid.NewGuid().ToString().ToLower(),
                    RecordId = role.RoleID,
                    NewValue = $"ROLENAME={role.RoleName}, PERMISSIONSCOPE={role.PermissionScope}",
                    OperatorId = operatorId
                }, transaction);
        
                transaction.Commit();
                return rows;
            } catch {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<string> AssignRoleWithAuditAsync(string userId, string newRoleId, string operatorId)
        {
            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();
            try {
                // 1. 获取当前角色（加锁防止并发）
                var oldRoleId = await conn.ExecuteScalarAsync<string>(
                    "SELECT ROLEID FROM SYS_USERS WHERE USERID = :UserId FOR UPDATE",
                    new { UserId = userId }, transaction);
        
                if (oldRoleId == null)
                    return "用户不存在";
        
                // 2. 检查是否相同角色
                if (oldRoleId == newRoleId)
                    return "";  // 无需操作
        
                // 3. 更新角色
                var rows = await conn.ExecuteAsync(
                    "UPDATE SYS_USERS SET ROLEID = :NewRoleId WHERE USERID = :UserId",
                    new { UserId = userId, NewRoleId = newRoleId }, transaction);
        
                if (rows == 0)
                    return "角色分配失败";
        
                // 4. 获取角色名称
                var oldRoleName = await conn.ExecuteScalarAsync<string>(
                    "SELECT ROLENAME FROM SYS_ROLES WHERE ROLEID = :RoleId",
                    new { RoleId = oldRoleId }, transaction);
                var newRoleName = await conn.ExecuteScalarAsync<string>(
                    "SELECT ROLENAME FROM SYS_ROLES WHERE ROLEID = :RoleId",
                    new { RoleId = newRoleId }, transaction);
        
                // 5. 写入审计日志
                const string auditSql = @"
                    INSERT INTO LOG_AUDITTRAILS (
                        LOGID, TABLENAME, RECORDID, ACTIONTYPE, OLDVALUE, NEWVALUE, OPERATORID, OPTIME
                    ) VALUES (
                        :LogId, 'SYS_USERS', :RecordId, 'UPDATE_ROLE', 
                        :OldValue, :NewValue, :OperatorId, SYSTIMESTAMP
                    )";
        
                await conn.ExecuteAsync(auditSql, new
                {
                    LogId = Guid.NewGuid().ToString().ToLower(),
                    RecordId = userId,
                    OldValue = $"ROLEID={oldRoleId}, ROLENAME={oldRoleName}",
                    NewValue = $"ROLEID={newRoleId}, ROLENAME={newRoleName}",
                    OperatorId = operatorId
                }, transaction);
        
                transaction.Commit();
                return "";  // 成功
            } catch (Exception ex) {
                transaction.Rollback();
                return $"分配角色失败: {ex.Message}";
            }
        }
    }
}
