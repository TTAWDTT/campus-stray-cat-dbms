using CampusStrayCatSystem.Models;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 失踪预警仓储实现。
    /// 这里把“最后目击”和“预警”分成两个清晰动作，便于验收时逐步检查。
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
                FROM CAT_MISSINGALERTS
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
                FROM CAT_MISSINGALERTS
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
                FROM CAT_MISSINGALERTS
                WHERE ALERTID = :AlertID";

            return await QuerySingleAsync(sql, new { AlertID = alertId });
        }

        public async Task<int> CreateSighting(CatSighting sighting)
        {
            sighting.SightingID = EnsureId(sighting.SightingID);

            const string sql = @"
                INSERT INTO CAT_SIGHTINGS (
                    SIGHTINGID,
                    CATID,
                    USERID,
                    AREAID,
                    LONGITUDE,
                    LATITUDE,
                    PHOTOURL,
                    SIGHTINGTIME,
                    REMARK
                ) VALUES (
                    :SightingID,
                    :CatID,
                    :UserID,
                    :AreaID,
                    :Longitude,
                    :Latitude,
                    :PhotoURL,
                    :SightingTime,
                    :Remark
                )";

            return await ExecuteAsync(sql, sighting);
        }

        public async Task<int> CreateAlert(CatMissingAlert alert)
        {
            alert.AlertID = EnsureId(alert.AlertID);
            alert.AlertStatus = NormalizeStatus(alert.AlertStatus, "PROCESSING");
            alert.AlertTime ??= DateTime.Now;

            const string sql = @"
                INSERT INTO CAT_MISSINGALERTS (
                    ALERTID,
                    CATID,
                    LASTSIGHTINGID,
                    LASTSIGHTINGTIME,
                    THRESHOLDDAYS,
                    ALERTTIME,
                    ALERTSTATUS,
                    HANDLERUSERID,
                    CLOSETIME,
                    REMARK
                ) VALUES (
                    :AlertID,
                    :CatID,
                    :LastSightingID,
                    :LastSightingTime,
                    :ThresholdDays,
                    :AlertTime,
                    :AlertStatus,
                    :HandlerUserID,
                    :CloseTime,
                    :Remark
                )";

            return await ExecuteAsync(sql, alert);
        }

        public async Task<int> UpdateStatus(string alertId, string alertStatus, string? handlerUserId, string? remark)
        {
            string normalizedStatus = NormalizeStatus(alertStatus, "PROCESSING");
            DateTime? closeTime = normalizedStatus is "FOUND" or "CLOSED" ? DateTime.Now : null;

            const string sql = @"
                UPDATE CAT_MISSINGALERTS
                SET ALERTSTATUS = :AlertStatus,
                    HANDLERUSERID = NVL(:HandlerUserID, HANDLERUSERID),
                    REMARK = NVL(:Remark, REMARK),
                    CLOSETIME = CASE
                        WHEN :CloseTime IS NOT NULL THEN :CloseTime
                        ELSE CLOSETIME
                    END
                WHERE ALERTID = :AlertID";

            return await ExecuteAsync(sql, new
            {
                AlertID = alertId,
                AlertStatus = normalizedStatus,
                HandlerUserID = handlerUserId,
                Remark = remark,
                CloseTime = closeTime
            });
        }

        private static string EnsureId(string? id)
        {
            return string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        }

        private static string NormalizeStatus(string? status, string fallback)
        {
            return string.IsNullOrWhiteSpace(status) ? fallback : status.Trim().ToUpperInvariant();
        }
    }
}
