using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        // 与 database/queries/a_group_advanced.sql 中的 VW_USER_ROLE_PROFILE 对齐。
        private const string UserRoleProfileSelect = @"
                SELECT USERID AS UserID,
                       ROLEID AS RoleID,
                       USERNAME AS Username,
                       PASSWORDHASH AS PasswordHash,
                       REALNAME AS RealName,
                       STUDENTNO AS StudentNo,
                       PHONE AS Phone,
                       VERIFYSTATUS AS VerifyStatus,
                       STATUS AS Status,
                       ROLENAME AS RoleName,
                       PERMISSIONSCOPE AS PermissionScope
                FROM VW_USER_ROLE_PROFILE";

        public UserRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<User?> GetById(string userId)
        {
            const string sql = UserRoleProfileSelect + @"
                WHERE USERID = :UserID";

            return await QuerySingleAsync(sql, new { UserID = userId });
        }

        public async Task<User?> GetByUsername(string username)
        {
            const string sql = UserRoleProfileSelect + @"
                WHERE UPPER(USERNAME) = UPPER(:Username)";

            return await QuerySingleAsync(sql, new { Username = username });
        }

        public async Task<IEnumerable<User>> GetAll(string? username, string? status, string? roleId)
        {
            const string sql = UserRoleProfileSelect + @"
                WHERE (:Username IS NULL OR UPPER(USERNAME) LIKE '%' || UPPER(:Username) || '%')
                  AND (:Status IS NULL OR STATUS = :Status)
                  AND (:RoleID IS NULL OR ROLEID = :RoleID)
                ORDER BY USERNAME";

            return await QueryAsync(sql, new { Username = username, Status = status, RoleID = roleId });
        }

        public async Task<int> Create(User user)
        {
            const string sql = @"
                INSERT INTO SYS_USERS (
                    USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, STUDENTNO, PHONE, VERIFYSTATUS, STATUS
                ) VALUES (
                    :UserID, :RoleID, :Username, :PasswordHash, :RealName, :StudentNo, :Phone, :VerifyStatus, :Status
                )";

            return await ExecuteAsync(sql, user);
        }

        public async Task<int> Update(User user)
        {
            const string sql = @"
                UPDATE SYS_USERS
                SET ROLEID = :RoleID,
                    REALNAME = :RealName,
                    STUDENTNO = :StudentNo,
                    PHONE = :Phone,
                    VERIFYSTATUS = :VerifyStatus,
                    STATUS = :Status
                WHERE USERID = :UserID";

            return await ExecuteAsync(sql, user);
        }

        public async Task<int> UpdateStatus(string userId, string status)
        {
            const string sql = @"
                UPDATE SYS_USERS
                SET STATUS = :Status
                WHERE USERID = :UserID";

            return await ExecuteAsync(sql, new { UserID = userId, Status = status });
        }

        public async Task<bool> Exists(string userId)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_USERS WHERE USERID = :UserID";
            var count = await QuerySingleAsync<int>(sql, new { UserID = userId });
            return count > 0;
        }

        public async Task<bool> UsernameExists(string username)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_USERS WHERE UPPER(USERNAME) = UPPER(:Username)";
            var count = await QuerySingleAsync<int>(sql, new { Username = username });
            return count > 0;
        }
    }
}
