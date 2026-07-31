using System.Text;
using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    public class NestMaintenanceRecordRepository
        : BaseRepository<NestMaintenanceRecord>, INestMaintenanceRecordRepository
    {
        private const string SelectColumns = @"
            SELECT MAINTENANCEID AS MaintenanceID,
                   POINTID AS PointID,
                   MATERIALTYPE AS MaterialType,
                   CHECKTIME AS CheckTime,
                   WEATHERCONDITION AS WeatherCondition,
                   DAMAGELEVEL AS DamageLevel,
                   ACTIONTYPE AS ActionType,
                   OPERATORUSERID AS OperatorUserID,
                   NEXTCHECKTIME AS NextCheckTime,
                   REMARK AS Remark
            FROM NEST_MAINTENANCERECORDS";

        public NestMaintenanceRecordRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<NestMaintenanceRecord>> GetAllAsync(
            string? pointId = null,
            string? damageLevel = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var sql = new StringBuilder(SelectColumns);
            sql.AppendLine(" WHERE 1 = 1");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(pointId))
            {
                sql.AppendLine(" AND POINTID = :PointID");
                parameters.Add("PointID", pointId.Trim());
            }

            if (!string.IsNullOrWhiteSpace(damageLevel))
            {
                sql.AppendLine(" AND DAMAGELEVEL = :DamageLevel");
                parameters.Add("DamageLevel", damageLevel.Trim());
            }

            if (from.HasValue)
            {
                sql.AppendLine(" AND CHECKTIME >= :FromTime");
                parameters.Add("FromTime", from.Value);
            }

            if (to.HasValue)
            {
                sql.AppendLine(" AND CHECKTIME <= :ToTime");
                parameters.Add("ToTime", to.Value);
            }

            sql.AppendLine(" ORDER BY CHECKTIME DESC NULLS LAST");
            return await QueryAsync(sql.ToString(), parameters);
        }

        public async Task<NestMaintenanceRecord?> GetByIdAsync(string id)
        {
            const string sql = SelectColumns + " WHERE MAINTENANCEID = :MaintenanceID";
            return await QuerySingleAsync(sql, new { MaintenanceID = id });
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_USERS WHERE USERID = :UserID";
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { UserID = userId }) > 0;
        }

        public async Task<int> CreateAsync(NestMaintenanceRecord record)
        {
            const string sql = @"
                INSERT INTO NEST_MAINTENANCERECORDS
                    (MAINTENANCEID, POINTID, MATERIALTYPE, CHECKTIME, WEATHERCONDITION,
                     DAMAGELEVEL, ACTIONTYPE, OPERATORUSERID, NEXTCHECKTIME, REMARK)
                VALUES
                    (:MaintenanceID, :PointID, :MaterialType, :CheckTime, :WeatherCondition,
                     :DamageLevel, :ActionType, :OperatorUserID, :NextCheckTime, :Remark)";

            return await ExecuteAsync(sql, record);
        }

        public async Task<int> UpdateAsync(NestMaintenanceRecord record)
        {
            const string sql = @"
                UPDATE NEST_MAINTENANCERECORDS
                SET POINTID = :PointID,
                    MATERIALTYPE = :MaterialType,
                    CHECKTIME = :CheckTime,
                    WEATHERCONDITION = :WeatherCondition,
                    DAMAGELEVEL = :DamageLevel,
                    ACTIONTYPE = :ActionType,
                    OPERATORUSERID = :OperatorUserID,
                    NEXTCHECKTIME = :NextCheckTime,
                    REMARK = :Remark
                WHERE MAINTENANCEID = :MaintenanceID";

            return await ExecuteAsync(sql, record);
        }

        public async Task<int> DeleteAsync(string id)
        {
            const string sql = @"
                DELETE FROM NEST_MAINTENANCERECORDS
                WHERE MAINTENANCEID = :MaintenanceID";

            return await ExecuteAsync(sql, new { MaintenanceID = id });
        }
    }
}
