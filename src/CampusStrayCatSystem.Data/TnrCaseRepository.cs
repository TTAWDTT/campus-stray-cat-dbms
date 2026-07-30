using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public class TnrCaseRepository : BaseRepository<TnrCase>, ITnrCaseRepository
    {
        public TnrCaseRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<TnrCase>> GetAll()
        {
            const string sql = @"
                SELECT CASEID AS CaseID,
                       CATID AS CatID,
                       RESPONSIBLEUSERID AS ResponsibleUserID,
                       CURRENTSTATUS AS CurrentStatus,
                       HOSPITALNAME AS HospitalName,
                       CAPTURETIME AS CaptureTime,
                       SURGERYTIME AS SurgeryTime,
                       RELEASETIME AS ReleaseTime,
                       TOTALCOST AS TotalCost
                FROM TNR_CASES
                ORDER BY CAPTURETIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        public async Task<TnrCase?> GetById(string id)
        {
            const string sql = @"
                SELECT CASEID AS CaseID,
                       CATID AS CatID,
                       RESPONSIBLEUSERID AS ResponsibleUserID,
                       CURRENTSTATUS AS CurrentStatus,
                       HOSPITALNAME AS HospitalName,
                       CAPTURETIME AS CaptureTime,
                       SURGERYTIME AS SurgeryTime,
                       RELEASETIME AS ReleaseTime,
                       TOTALCOST AS TotalCost
                FROM TNR_CASES
                WHERE CASEID = :CaseID";

            return await QuerySingleAsync(sql, new { CaseID = id });
        }

        public async Task<int> Create(TnrCase tnrCase)
        {
            tnrCase.CaseID = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO TNR_CASES (CASEID, CATID, RESPONSIBLEUSERID, CURRENTSTATUS, HOSPITALNAME,
                                       CAPTURETIME, SURGERYTIME, RELEASETIME, TOTALCOST)
                VALUES (:CaseID, :CatID, :ResponsibleUserID, :CurrentStatus, :HospitalName,
                        :CaptureTime, :SurgeryTime, :ReleaseTime, :TotalCost)";

            return await ExecuteAsync(sql, new
            {
                tnrCase.CaseID,
                tnrCase.CatID,
                tnrCase.ResponsibleUserID,
                tnrCase.CurrentStatus,
                tnrCase.HospitalName,
                tnrCase.CaptureTime,
                tnrCase.SurgeryTime,
                tnrCase.ReleaseTime,
                tnrCase.TotalCost
            });
        }

        public async Task<int> Update(TnrCase tnrCase)
        {
            const string sql = @"
                UPDATE TNR_CASES
                SET CATID = :CatID,
                    RESPONSIBLEUSERID = :ResponsibleUserID,
                    CURRENTSTATUS = :CurrentStatus,
                    HOSPITALNAME = :HospitalName,
                    CAPTURETIME = :CaptureTime,
                    SURGERYTIME = :SurgeryTime,
                    RELEASETIME = :ReleaseTime,
                    TOTALCOST = :TotalCost
                WHERE CASEID = :CaseID";

            return await ExecuteAsync(sql, new
            {
                tnrCase.CatID,
                tnrCase.ResponsibleUserID,
                tnrCase.CurrentStatus,
                tnrCase.HospitalName,
                tnrCase.CaptureTime,
                tnrCase.SurgeryTime,
                tnrCase.ReleaseTime,
                tnrCase.TotalCost,
                tnrCase.CaseID
            });
        }

        public async Task<int> Delete(string id)
        {
            const string sql = @"DELETE FROM TNR_CASES WHERE CASEID = :CaseID";

            return await ExecuteAsync(sql, new { CaseID = id });
        }
    }
}
