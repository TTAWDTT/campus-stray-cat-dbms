using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 用户黑名单数据访问实现
    /// </summary>
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
                SELECT * FROM USER_BLACKLIST 
                WHERE 1=1
                AND (:UserId IS NULL OR UserID = :UserId)
                AND (:Status IS NULL OR Status = :Status)
                ORDER BY CreatedAt DESC
                OFFSET (:Page - 1) * :PageSize ROWS
                FETCH NEXT :PageSize ROWS ONLY
            ";

            var parameters = new
            {
                UserId = userId,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            return await conn.QueryAsync<UserBlacklist>(sql, parameters);
        }

        public async Task<UserBlacklist> GetByIdAsync(string blacklistId)
        {
            using var conn = new OracleConnection(_connectionString);
            
            var sql = "SELECT * FROM USER_BLACKLIST WHERE BlacklistID = :BlacklistID";
            
            return await conn.QueryFirstOrDefaultAsync<UserBlacklist>(
                sql, 
                new { BlacklistID = blacklistId }
            );
        }

        public async Task AddAsync(UserBlacklist record)
        {
            using var conn = new OracleConnection(_connectionString);
            
            // 调用存储过程
            var parameters = new OracleDynamicParameters();
            parameters.Add("p_UserID", record.UserID);
            parameters.Add("p_ReasonType", record.ReasonType);
            parameters.Add("p_ReasonDetail", record.ReasonDetail);
            parameters.Add("p_ApplicationID", record.ApplicationID ?? (object)DBNull.Value);
            parameters.Add("p_CreatedBy", record.CreatedBy);
            parameters.Add("p_Result", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            await conn.ExecuteAsync(
                "SP_ADD_USER_BLACKLIST",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var result = parameters.Get<string>("p_Result");
            if (!string.IsNullOrEmpty(result))
            {
                throw new Exception(result);
            }
        }

        public async Task ReleaseAsync(string blacklistId, string releasedBy)
        {
            using var conn = new OracleConnection(_connectionString);
            
            var parameters = new OracleDynamicParameters();
            parameters.Add("p_BlacklistID", blacklistId);
            parameters.Add("p_ReleasedBy", releasedBy);
            parameters.Add("p_Result", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            await conn.ExecuteAsync(
                "SP_RELEASE_USER_BLACKLIST",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var result = parameters.Get<string>("p_Result");
            if (!string.IsNullOrEmpty(result))
            {
                throw new Exception(result);
            }
        }

        public async Task<bool> HasActiveBlacklistAsync(string userId)
        {
            using var conn = new OracleConnection(_connectionString);
            
            var sql = @"
                SELECT COUNT(1) 
                FROM USER_BLACKLIST 
                WHERE UserID = :UserId AND Status = 'Active'
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
                    ReasonType as ReasonType,
                    ReasonDetail as ReasonDetail,
                    CreatedAt as CreatedAt
                FROM USER_BLACKLIST 
                WHERE UserID = :UserId AND Status = 'Active'
                ORDER BY CreatedAt DESC
                FETCH FIRST 1 ROW ONLY
            ";

            return await conn.QueryFirstOrDefaultAsync<BlacklistStatusDto>(
                sql, 
                new { UserId = userId }
            );
        }
    }
}