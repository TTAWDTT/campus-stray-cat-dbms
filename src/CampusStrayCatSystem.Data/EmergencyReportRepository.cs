using CampusStrayCatSystem.Models;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 紧急救助上报仓储实现。
    /// 这里保留最常用的查询和更新动作，方便前端直接对接。
    /// </summary>
    public class EmergencyReportRepository : BaseRepository<EmergencyReport>, IEmergencyReportRepository
    {
        public EmergencyReportRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<IEnumerable<EmergencyReport>> GetAll()
        {
            const string sql = @"
                SELECT REPORTID AS ReportID,
                       REPORTERUSERID AS ReporterUserID,
                       AREAID AS AreaID,
                       ANIMALTYPE AS AnimalType,
                       PHOTOURL AS PhotoURL,
                       LONGITUDE AS Longitude,
                       LATITUDE AS Latitude,
                       REPORTTIME AS ReportTime,
                       URGENCYLEVEL AS UrgencyLevel,
                       PROCESSSTATUS AS ProcessStatus,
                       HANDLERUSERID AS HandlerUserID,
                       PROCESSRESULT AS ProcessResult
                FROM EMERGENCY_REPORTS
                ORDER BY REPORTTIME DESC";

            return await QueryAsync(sql);
        }

        public async Task<EmergencyReport?> GetById(string reportId)
        {
            const string sql = @"
                SELECT REPORTID AS ReportID,
                       REPORTERUSERID AS ReporterUserID,
                       AREAID AS AreaID,
                       ANIMALTYPE AS AnimalType,
                       PHOTOURL AS PhotoURL,
                       LONGITUDE AS Longitude,
                       LATITUDE AS Latitude,
                       REPORTTIME AS ReportTime,
                       URGENCYLEVEL AS UrgencyLevel,
                       PROCESSSTATUS AS ProcessStatus,
                       HANDLERUSERID AS HandlerUserID,
                       PROCESSRESULT AS ProcessResult
                FROM EMERGENCY_REPORTS
                WHERE REPORTID = :ReportID";

            return await QuerySingleAsync(sql, new { ReportID = reportId });
        }

        public async Task<int> Create(EmergencyReport report)
        {
            report.ReportID = EnsureId(report.ReportID);
            report.ProcessStatus = NormalizeStatus(report.ProcessStatus, "SUBMITTED");
            report.UrgencyLevel = NormalizeStatus(report.UrgencyLevel, "LOW");
            report.ReportTime ??= DateTime.Now;

            const string sql = @"
                INSERT INTO EMERGENCY_REPORTS (
                    REPORTID,
                    REPORTERUSERID,
                    AREAID,
                    ANIMALTYPE,
                    PHOTOURL,
                    LONGITUDE,
                    LATITUDE,
                    REPORTTIME,
                    URGENCYLEVEL,
                    PROCESSSTATUS,
                    HANDLERUSERID,
                    PROCESSRESULT
                ) VALUES (
                    :ReportID,
                    :ReporterUserID,
                    :AreaID,
                    :AnimalType,
                    :PhotoURL,
                    :Longitude,
                    :Latitude,
                    :ReportTime,
                    :UrgencyLevel,
                    :ProcessStatus,
                    :HandlerUserID,
                    :ProcessResult
                )";

            return await ExecuteAsync(sql, report);
        }

        public async Task<int> AssignHandler(string reportId, string? handlerUserId)
        {
            const string sql = @"
                UPDATE EMERGENCY_REPORTS
                SET HANDLERUSERID = :HandlerUserID,
                    PROCESSSTATUS = 'ASSIGNED'
                WHERE REPORTID = :ReportID";

            return await ExecuteAsync(sql, new
            {
                ReportID = reportId,
                HandlerUserID = handlerUserId
            });
        }

        public async Task<int> UpdateStatus(string reportId, string status, string? processResult)
        {
            const string sql = @"
                UPDATE EMERGENCY_REPORTS
                SET PROCESSSTATUS = :ProcessStatus,
                    PROCESSRESULT = :ProcessResult
                WHERE REPORTID = :ReportID";

            return await ExecuteAsync(sql, new
            {
                ReportID = reportId,
                ProcessStatus = NormalizeStatus(status, "SUBMITTED"),
                ProcessResult = processResult
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
