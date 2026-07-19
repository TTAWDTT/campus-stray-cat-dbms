using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    public abstract class BaseRepository<T> where T : class
    {
        private readonly string _connectionString;

        protected BaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Oracle")
                ?? throw new InvalidOperationException("Connection string 'Oracle' not found.");
        }

        protected IDbConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }

        protected async Task<IEnumerable<T>> QueryAsync(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<T>(sql, param);
        }

        protected async Task<T?> QuerySingleAsync(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
        }
        protected async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, param);
        }
    }
}
