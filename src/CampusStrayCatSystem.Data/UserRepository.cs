using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<User?> GetById(string userId)
        {
            const string sql = @"
                SELECT u.USERID AS UserID,
                       u.ROLEID AS RoleID,
                       u.USERNAME AS Username,
                       u.PASSWORDHASH AS PasswordHash,
                       u.REALNAME AS RealName,
                       u.STUDENTNO AS StudentNo,
                       u.PHONE AS Phone,
                       u.VERIFYSTATUS AS VerifyStatus,
                       u.STATUS AS Status,
                       r.ROLENAME AS RoleName,
                       r.PERMISSIONSCOPE AS PermissionScope
                FROM SYS_USERS u
                JOIN SYS_ROLES r ON r.ROLEID = u.ROLEID
                WHERE u.USERID = :UserID";

            return await QuerySingleAsync(sql, new { UserID = userId });
        }

        public async Task<User?> GetByUsername(string username)
        {
            const string sql = @"
                SELECT u.USERID AS UserID,
                       u.ROLEID AS RoleID,
                       u.USERNAME AS Username,
                       u.PASSWORDHASH AS PasswordHash,
                       u.REALNAME AS RealName,
                       u.STUDENTNO AS StudentNo,
                       u.PHONE AS Phone,
                       u.VERIFYSTATUS AS VerifyStatus,
                       u.STATUS AS Status,
                       r.ROLENAME AS RoleName,
                       r.PERMISSIONSCOPE AS PermissionScope
                FROM SYS_USERS u
                JOIN SYS_ROLES r ON r.ROLEID = u.ROLEID
                WHERE UPPER(u.USERNAME) = UPPER(:Username)";

            return await QuerySingleAsync(sql, new { Username = username });
        }

        public async Task<IEnumerable<User>> GetAll(string? username, string? status, string? roleId)
        {
            const string sql = @"
                SELECT u.USERID AS UserID,
                       u.ROLEID AS RoleID,
                       u.USERNAME AS Username,
                       u.PASSWORDHASH AS PasswordHash,
                       u.REALNAME AS RealName,
                       u.STUDENTNO AS StudentNo,
                       u.PHONE AS Phone,
                       u.VERIFYSTATUS AS VerifyStatus,
                       u.STATUS AS Status,
                       r.ROLENAME AS RoleName,
                       r.PERMISSIONSCOPE AS PermissionScope
                FROM SYS_USERS u
                JOIN SYS_ROLES r ON r.ROLEID = u.ROLEID
                WHERE (:Username IS NULL OR UPPER(u.USERNAME) LIKE '%' || UPPER(:Username) || '%')
                  AND (:Status IS NULL OR UPPER(u.STATUS) = UPPER(:Status))
                  AND (:RoleID IS NULL OR u.ROLEID = :RoleID)
                ORDER BY u.USERNAME";

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
