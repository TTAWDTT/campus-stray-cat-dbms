using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Models.DTOs;

namespace CampusStrayCatSystem.Data
{
    public class UserBlacklistRepository : IUserBlacklistRepository
    {
        private readonly string _connectionString;

        public UserBlacklistRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<UserBlacklist>> GetAllAsync(
            string userId = null,
            string status = null,
            int page = 1,
            int pageSize = 20)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                SELECT
                    BlacklistID,
                    UserID,
                    ReasonType,
                    ReasonDetail,
                    ApplicationID,
                    CreateUserID,
                    CreateTime,
                    BlacklistStatus,
                    ReleaseTime,
                    ReleasedBy
                FROM USER_BLACKLIST
                WHERE 1=1
            ";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(userId))
            {
                sql += " AND UserID = :UserId";
                parameters.Add("UserId", userId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND BlacklistStatus = :Status";
                parameters.Add("Status", status);
            }

            sql += @"
                ORDER BY CreateTime DESC
                OFFSET :Offset ROWS
                FETCH NEXT :PageSize ROWS ONLY
            ";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            return await conn.QueryAsync<UserBlacklist>(sql, parameters);
        }

        public async Task<UserBlacklist> GetByIdAsync(string blacklistId)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                SELECT
                    BlacklistID,
                    UserID,
                    ReasonType,
                    ReasonDetail,
                    ApplicationID,
                    CreateUserID,
                    CreateTime,
                    BlacklistStatus,
                    ReleaseTime,
                    ReleasedBy
                FROM USER_BLACKLIST
                WHERE BlacklistID = :BlacklistID
            ";

            return await conn.QueryFirstOrDefaultAsync<UserBlacklist>(sql, new { BlacklistID = blacklistId });
        }

        public async Task AddAsync(UserBlacklist record)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                INSERT INTO USER_BLACKLIST (
                    BlacklistID,
                    UserID,
                    ReasonType,
                    ReasonDetail,
                    ApplicationID,
                    CreateUserID,
                    CreateTime,
                    BlacklistStatus
                ) VALUES (
                    :BlacklistID,
                    :UserID,
                    :ReasonType,
                    :ReasonDetail,
                    :ApplicationID,
                    :CreateUserID,
                    :CreateTime,
                    :BlacklistStatus
                )";

            record.BlacklistID = string.IsNullOrEmpty(record.BlacklistID)
                ? Guid.NewGuid().ToString()
                : record.BlacklistID;
            record.CreateTime = DateTime.Now;
            record.BlacklistStatus = "Active";

            await conn.ExecuteAsync(sql, record);
        }

        public async Task ReleaseAsync(string blacklistId, string releasedBy)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                UPDATE USER_BLACKLIST
                SET BlacklistStatus = 'Released',
                    ReleaseTime = SYSTIMESTAMP,
                    ReleasedBy = :ReleasedBy
                WHERE BlacklistID = :BlacklistID
                  AND BlacklistStatus = 'Active'
            ";

            var rowsAffected = await conn.ExecuteAsync(sql, new
            {
                BlacklistID = blacklistId,
                ReleasedBy = releasedBy
            });

            if (rowsAffected == 0)
            {
                throw new Exception("黑名单记录不存在或已被解除");
            }
        }

        public async Task<bool> HasActiveBlacklistAsync(string userId)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                SELECT COUNT(1)
                FROM USER_BLACKLIST
                WHERE UserID = :UserId
                AND BlacklistStatus = 'Active'
            ";

            var count = await conn.ExecuteScalarAsync<int>(sql, new { UserId = userId });
            return count > 0;
        }

        public async Task<BlacklistStatusDto> GetActiveStatusByUserIdAsync(string userId)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                SELECT
                    UserID as UserId,
                    'Y' as IsBlacklisted,
                    BlacklistID as BlacklistId,
                    ReasonType as ReasonType,
                    ReasonDetail as ReasonDetail,
                    CreateTime as BlacklistedAt
                FROM USER_BLACKLIST
                WHERE UserID = :UserId
                AND BlacklistStatus = 'Active'
                ORDER BY CreateTime DESC
                FETCH FIRST 1 ROW ONLY
            ";

            var result = await conn.QueryFirstOrDefaultAsync<BlacklistStatusDto>(sql, new { UserId = userId });

            if (result == null)
            {
                return new BlacklistStatusDto
                {
                    UserId = userId,
                    IsBlacklisted = false
                };
            }

            return result;
        }

        public async Task<int> GetTotalCountAsync(string userId = null, string status = null)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                SELECT COUNT(1)
                FROM USER_BLACKLIST
                WHERE 1=1
            ";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(userId))
            {
                sql += " AND UserID = :UserId";
                parameters.Add("UserId", userId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND BlacklistStatus = :Status";
                parameters.Add("Status", status);
            }

            return await conn.ExecuteScalarAsync<int>(sql, parameters);
        }
    }
}