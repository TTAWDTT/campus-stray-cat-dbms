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
            // 所有仓储统一从配置里取 Oracle 连接串。
            return new OracleConnection(_connectionString);
        }

        protected async Task<IEnumerable<T>> QueryAsync(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<T>(sql, param);
        }

        protected async Task<IEnumerable<TModel>> QueryAsync<TModel>(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<TModel>(sql, param);
        }

        protected async Task<T?> QuerySingleAsync(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        protected async Task<TResult?> QuerySingleAsync<TResult>(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TResult>(sql, param);
        }
        protected async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try {
                var affected = await connection.ExecuteAsync(sql, param, transaction);
                transaction.Commit();
                return affected;
            } catch {
                transaction.Rollback();
                throw;
            }
        }

        protected async Task<int> ExecuteAsync(IDbConnection connection, IDbTransaction transaction, string sql, object? param = null)
        {
            return await connection.ExecuteAsync(sql, param, transaction);
        }

        protected async Task<int> ExecuteStoredProcedureAsync(string procedureName, object param)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try {
                var affected = await connection.ExecuteAsync(procedureName, param, transaction,
                    commandType: CommandType.StoredProcedure);
                transaction.Commit();
                return affected;
            } catch {
                transaction.Rollback();
                throw;
            }
        }

        protected async Task<TResult?> QuerySingleAsync<TResult>(IDbConnection connection, string sql, object? param = null)
        {
            return await connection.QueryFirstOrDefaultAsync<TResult>(sql, param);
        }
    }
}
