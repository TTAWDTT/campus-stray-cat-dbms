using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 紧急救助上报仓储实现。
    /// 查询使用视图，提交、分配和状态更新调用 Oracle Package。
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
                FROM VW_EMERGENCY_REPORTS
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
                FROM VW_EMERGENCY_REPORTS
                WHERE REPORTID = :ReportID";

            return await QuerySingleAsync(sql, new { ReportID = reportId });
        }

        public async Task<int> Create(EmergencyReport report)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_REPORTERUSERID", report.ReporterUserID, DbType.String);
            parameters.Add("P_AREAID", report.AreaID, DbType.String);
            parameters.Add("P_ANIMALTYPE", report.AnimalType, DbType.String);
            parameters.Add("P_PHOTOURL", report.PhotoURL, DbType.String);
            parameters.Add("P_LONGITUDE", report.Longitude, DbType.Decimal);
            parameters.Add("P_LATITUDE", report.Latitude, DbType.Decimal);
            parameters.Add("P_URGENCYLEVEL", report.UrgencyLevel, DbType.String);
            parameters.Add("O_REPORTID", dbType: DbType.String, direction: ParameterDirection.Output, size: 36);

            var rows = await ExecuteStoredProcedureAsync(
                "PKG_RESCUE_CARE.SUBMIT_EMERGENCY_REPORT",
                parameters);

            report.ReportID = parameters.Get<string>("O_REPORTID");
            report.ReportTime = DateTime.Now;
            report.ProcessStatus = "SUBMITTED";

            return rows;
        }

        public async Task<int> AssignHandler(string reportId, string? handlerUserId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_REPORTID", reportId, DbType.String);
            parameters.Add("P_HANDLERUSERID", handlerUserId, DbType.String);

            return await ExecuteStoredProcedureAsync(
                "PKG_RESCUE_CARE.ASSIGN_EMERGENCY_REPORT",
                parameters);
        }

        public async Task<int> UpdateStatus(string reportId, string status, string? processResult)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_REPORTID", reportId, DbType.String);
            parameters.Add("P_PROCESSSTATUS", status, DbType.String);
            parameters.Add("P_PROCESSRESULT", processResult, DbType.String);

            return await ExecuteStoredProcedureAsync(
                "PKG_RESCUE_CARE.UPDATE_EMERGENCY_STATUS",
                parameters);
        }
    }
}
