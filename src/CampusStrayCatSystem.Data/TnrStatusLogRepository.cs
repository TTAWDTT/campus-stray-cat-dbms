using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public class TnrStatusLogRepository : BaseRepository<TnrStatusLog>, ITnrStatusLogRepository
    {
        public TnrStatusLogRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<TnrStatusLog>> GetByCaseId(string caseId)
        {
            const string sql = @"
                SELECT LOGID AS LogID,
                       CASEID AS CaseID,
                       FROMSTATUS AS FromStatus,
                       TOSTATUS AS ToStatus,
                       OPERATORID AS OperatorID,
                       OPTIME AS OpTime,
                       REMARK AS Remark
                FROM TNR_STATUSLOGS
                WHERE CASEID = :CaseID
                ORDER BY OPTIME ASC";

            return await QueryAsync(sql, new { CaseID = caseId });
        }

        public async Task<TnrStatusLog?> GetById(string logId)
        {
            const string sql = @"
                SELECT LOGID AS LogID,
                       CASEID AS CaseID,
                       FROMSTATUS AS FromStatus,
                       TOSTATUS AS ToStatus,
                       OPERATORID AS OperatorID,
                       OPTIME AS OpTime,
                       REMARK AS Remark
                FROM TNR_STATUSLOGS
                WHERE LOGID = :LogID";

            return await QuerySingleAsync(sql, new { LogID = logId });
        }

        public async Task<int> Create(TnrStatusLog log)
        {
            log.LogID = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO TNR_STATUSLOGS (LOGID, CASEID, FROMSTATUS, TOSTATUS, OPERATORID, OPTIME, REMARK)
                VALUES (:LogID, :CaseID, :FromStatus, :ToStatus, :OperatorID, SYSDATE, :Remark)";

            return await ExecuteAsync(sql, new
            {
                log.LogID,
                log.CaseID,
                log.FromStatus,
                log.ToStatus,
                log.OperatorID,
                log.Remark
            });
        }
    }
}
