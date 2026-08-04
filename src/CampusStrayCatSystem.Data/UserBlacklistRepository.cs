using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    public class UserBlacklistRepository : IUserBlacklistRepository
    {
        private const string ActiveStatus = "ACTIVE";
        private readonly string _connectionString;

        public UserBlacklistRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Oracle")
                ?? throw new InvalidOperationException("Connection string 'Oracle' not found.");
        }

        public async Task<IEnumerable<UserBlacklist>> GetAllAsync(
            string userId = null,
            string status = null,
            string keyword = null,
            int page = 1,
            int pageSize = 20)
        {
            page = Math.Max(1, Math.Min(page, 1_000_000));
            pageSize = Math.Clamp(pageSize, 1, 100); // 先兜住分页参数，Oracle 不喜欢负 offset
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                SELECT
                    BlacklistID,
                    UserID,
                    ReasonType,
                    ReasonDetail,
                    RELATEDAPPLICATIONID AS ApplicationID,
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
                sql += " AND UPPER(BlacklistStatus) = :Status";
                parameters.Add("Status", status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND (UPPER(ReasonType) LIKE '%' || UPPER(:Keyword) || '%' OR UPPER(ReasonDetail) LIKE '%' || UPPER(:Keyword) || '%')";
                parameters.Add("Keyword", keyword.Trim());
            }

            sql += @"
                ORDER BY CreateTime DESC
                OFFSET :Offset ROWS
                FETCH NEXT :PageSize ROWS ONLY
            ";

            parameters.Add("Offset", (long)(page - 1) * pageSize);
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
                    RELATEDAPPLICATIONID AS ApplicationID,
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

        public async Task<bool> AddAsync(UserBlacklist record)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                INSERT INTO USER_BLACKLIST (
                    BlacklistID,
                    UserID,
                    ReasonType,
                    ReasonDetail,
                    RELATEDAPPLICATIONID,
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
            record.BlacklistStatus = ActiveStatus;

            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();
            try {
                var userId = await conn.ExecuteScalarAsync<string>(
                    "SELECT USERID FROM SYS_USERS WHERE USERID = :UserID FOR UPDATE",
                    new { record.UserID }, transaction);
                if (userId == null) throw new InvalidOperationException("用户不存在");

                var activeCount = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM USER_BLACKLIST WHERE USERID = :UserID AND UPPER(BLACKLISTSTATUS) = 'ACTIVE'",
                    new { record.UserID }, transaction);
                if (activeCount > 0) {
                    transaction.Commit();
                    return false;
                }

                await conn.ExecuteAsync(sql, record, transaction);
                transaction.Commit();
                return true;
            } catch {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> ApplicationExistsAsync(string applicationId)
        {
            using var conn = new OracleConnection(_connectionString);
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM ADOPT_APPLICATIONS WHERE APPLICATIONID = :ApplicationID",
                new { ApplicationID = applicationId });
            return count > 0;
        }

        public async Task ReleaseAsync(string blacklistId, string releasedBy)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                UPDATE USER_BLACKLIST
                SET BlacklistStatus = 'RELEASED',
                    ReleaseTime = SYSTIMESTAMP,
                    ReleasedBy = :ReleasedBy
                WHERE BlacklistID = :BlacklistID
                  AND UPPER(BlacklistStatus) = 'ACTIVE'
            ";

            await conn.OpenAsync(); using var transaction = conn.BeginTransaction();
            int rowsAffected;
            try {
                rowsAffected = await conn.ExecuteAsync(sql, new
                {
                    BlacklistID = blacklistId,
                    ReleasedBy = releasedBy
                }, transaction);

                if (rowsAffected == 0) throw new Exception("黑名单记录不存在或已被解除");

                transaction.Commit();
            } catch {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> HasActiveBlacklistAsync(string userId)
        {
            using var conn = new OracleConnection(_connectionString);

            var sql = @"
                SELECT COUNT(1)
                FROM USER_BLACKLIST
                WHERE UserID = :UserId
                AND UPPER(BlacklistStatus) = 'ACTIVE'
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
                    1 as IsBlacklisted,
                    BlacklistID as BlacklistId,
                    ReasonType as ReasonType,
                    ReasonDetail as ReasonDetail,
                    CreateTime as BlacklistedAt
                FROM USER_BLACKLIST
                WHERE UserID = :UserId
                AND UPPER(BlacklistStatus) = 'ACTIVE'
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

        public async Task<int> GetTotalCountAsync(string userId = null, string status = null, string keyword = null)
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
                sql += " AND UPPER(BlacklistStatus) = :Status";
                parameters.Add("Status", status.Trim().ToUpperInvariant());
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND (UPPER(ReasonType) LIKE '%' || UPPER(:Keyword) || '%' OR UPPER(ReasonDetail) LIKE '%' || UPPER(:Keyword) || '%')";
                parameters.Add("Keyword", keyword.Trim());
            }

            return await conn.ExecuteScalarAsync<int>(sql, parameters);
        }
    }
}
