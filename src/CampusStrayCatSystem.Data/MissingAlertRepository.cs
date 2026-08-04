using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 失踪预警仓储实现。
    /// 查询使用视图，目击记录、预警创建和状态更新调用 Oracle Package。
    /// </summary>
    public class MissingAlertRepository : BaseRepository<CatMissingAlert>, IMissingAlertRepository
    {
        public MissingAlertRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<IEnumerable<CatMissingAlert>> GetAll()
        {
            const string sql = @"
                SELECT ALERTID AS AlertID,
                       CATID AS CatID,
                       LASTSIGHTINGID AS LastSightingID,
                       LASTSIGHTINGTIME AS LastSightingTime,
                       THRESHOLDDAYS AS ThresholdDays,
                       ALERTTIME AS AlertTime,
                       ALERTSTATUS AS AlertStatus,
                       HANDLERUSERID AS HandlerUserID,
                       CLOSETIME AS CloseTime,
                       REMARK AS Remark
                FROM VW_MISSING_ALERTS
                ORDER BY ALERTTIME DESC";

            return await QueryAsync(sql);
        }

        public async Task<IEnumerable<CatMissingAlert>> GetByCatId(string catId)
        {
            const string sql = @"
                SELECT ALERTID AS AlertID,
                       CATID AS CatID,
                       LASTSIGHTINGID AS LastSightingID,
                       LASTSIGHTINGTIME AS LastSightingTime,
                       THRESHOLDDAYS AS ThresholdDays,
                       ALERTTIME AS AlertTime,
                       ALERTSTATUS AS AlertStatus,
                       HANDLERUSERID AS HandlerUserID,
                       CLOSETIME AS CloseTime,
                       REMARK AS Remark
                FROM VW_MISSING_ALERTS
                WHERE CATID = :CatID
                ORDER BY ALERTTIME DESC";

            return await QueryAsync(sql, new { CatID = catId });
        }

        public async Task<CatMissingAlert?> GetById(string alertId)
        {
            const string sql = @"
                SELECT ALERTID AS AlertID,
                       CATID AS CatID,
                       LASTSIGHTINGID AS LastSightingID,
                       LASTSIGHTINGTIME AS LastSightingTime,
                       THRESHOLDDAYS AS ThresholdDays,
                       ALERTTIME AS AlertTime,
                       ALERTSTATUS AS AlertStatus,
                       HANDLERUSERID AS HandlerUserID,
                       CLOSETIME AS CloseTime,
                       REMARK AS Remark
                FROM VW_MISSING_ALERTS
                WHERE ALERTID = :AlertID";

            return await QuerySingleAsync(sql, new { AlertID = alertId });
        }

        public async Task<int> CreateSighting(CatSighting sighting)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_CATID", sighting.CatID, DbType.String);
            parameters.Add("P_USERID", sighting.UserID, DbType.String);
            parameters.Add("P_AREAID", sighting.AreaID, DbType.String);
            parameters.Add("P_LONGITUDE", sighting.Longitude, DbType.Decimal);
            parameters.Add("P_LATITUDE", sighting.Latitude, DbType.Decimal);
            parameters.Add("P_PHOTOURL", sighting.PhotoUrl, DbType.String);
            parameters.Add("P_SIGHTINGTIME", sighting.SightingTime, DbType.DateTime);
            parameters.Add("P_REMARK", sighting.Remark, DbType.String);
            parameters.Add("O_SIGHTINGID", dbType: DbType.String, direction: ParameterDirection.Output, size: 36);

            var rows = await ExecuteStoredProcedureAsync(
                "PKG_RESCUE_CARE.CREATE_SIGHTING",
                parameters);

            sighting.SightingID = parameters.Get<string>("O_SIGHTINGID");

            return rows;
        }

        public async Task<int> CreateAlert(CatMissingAlert alert)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_CATID", alert.CatID, DbType.String);
            parameters.Add("P_LASTSIGHTINGID", alert.LastSightingID, DbType.String);
            parameters.Add("P_LASTSIGHTINGTIME", alert.LastSightingTime, DbType.DateTime);
            parameters.Add("P_THRESHOLDDAYS", alert.ThresholdDays, DbType.Int32);
            parameters.Add("P_HANDLERUSERID", alert.HandlerUserID, DbType.String);
            parameters.Add("P_REMARK", alert.Remark, DbType.String);
            parameters.Add("O_ALERTID", dbType: DbType.String, direction: ParameterDirection.Output, size: 36);

            var rows = await ExecuteStoredProcedureAsync(
                "PKG_RESCUE_CARE.CREATE_MISSING_ALERT",
                parameters);

            alert.AlertID = parameters.Get<string>("O_ALERTID");
            alert.AlertStatus = "PROCESSING";
            alert.AlertTime = DateTime.Now;

            return rows;
        }

        public async Task<int> UpdateStatus(string alertId, string alertStatus, string? handlerUserId, string? remark)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_ALERTID", alertId, DbType.String);
            parameters.Add("P_ALERTSTATUS", alertStatus, DbType.String);
            parameters.Add("P_HANDLERUSERID", handlerUserId, DbType.String);
            parameters.Add("P_REMARK", remark, DbType.String);

            return await ExecuteStoredProcedureAsync(
                "PKG_RESCUE_CARE.UPDATE_MISSING_STATUS",
                parameters);
        }
    }
}
