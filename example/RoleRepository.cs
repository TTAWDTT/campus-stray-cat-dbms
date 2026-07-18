using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Cat.Models;

namespace Cat.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Oracle");
        }

        private IDbConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }

        public async Task<IEnumerable<Role>> GetAll()
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT ROLE_ID AS RoleID,
                       ROLE_NAME AS RoleName,
                       DESCRIPTION AS Description,
                       PERMISSIONSCOPE AS PermissionScope
                FROM ROLES
                ORDER BY CREATED_DATE DESC";

            return await connection.QueryAsync<Role>(sql);
        }

        public async Task<Role> GetByIdRole(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                SELECT ROLE_ID AS RoleID,
                       ROLE_NAME AS RoleName,
                       DESCRIPTION AS Description,
                       PERMISSIONSCOPE AS PermissionScope
                FROM ROLES
                WHERE ROLE_ID = :RoleID";

            return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleID = id });
        }

        public async Task<int> CreateRole(Role role)
        {
            using var connection = CreateConnection();
            const string sql = @"
                INSERT INTO ROLES (ROLE_ID, ROLE_NAME, DESCRIPTION, PERMISSIONSCOPE)
                VALUES (ROLES_SEQ.NEXTVAL, :RoleName, :Description, :PermissionScope)";

            return await connection.ExecuteAsync(sql, new
            {
                role.RoleName,
                role.Description,
                role.PermissionScope
            });
        }

        public async Task<int> UpdateRole(Role role)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE ROLES
                SET ROLE_NAME = :RoleName,
                    DESCRIPTION = :Description,
                    PERMISSIONSCOPE = :PermissionScope
                WHERE ROLE_ID = :RoleID";

            return await connection.ExecuteAsync(sql, new
            {
                role.RoleName,
                role.Description,
                role.PermissionScope,
                role.RoleID
            });
        }

        public async Task<int> DeleteRole(int id)
        {
            using var connection = CreateConnection();
            const string sql = @"
                UPDATE ROLES
                SET IS_ACTIVE = 0
                WHERE ROLE_ID = :RoleID";

            return await connection.ExecuteAsync(sql, new { RoleID = id });
        }
    }
}