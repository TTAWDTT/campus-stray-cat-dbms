using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    public class MedHealthRecordRepository : BaseRepository<MedHealthRecord>, IMedHealthRecordRepository
    {
        public MedHealthRecordRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<MedHealthRecord>> GetAll()
        {
            const string sql = @"
                SELECT RECORDID AS RecordID,
                       CATID AS CatID,
                       RECORDTYPE AS RecordType,
                       HOSPITALNAME AS HospitalName,
                       DIAGNOSIS AS Diagnosis,
                       RECORDDATE AS RecordDate,
                       NEXTDUEDATE AS NextDueDate,
                       ATTACHMENTURL AS AttachmentUrl
                FROM MED_HEALTHRECORDS
                ORDER BY RECORDDATE DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        public async Task<IEnumerable<MedHealthRecord>> GetByCatId(string catId)
        {
            const string sql = @"
                SELECT RECORDID AS RecordID,
                       CATID AS CatID,
                       RECORDTYPE AS RecordType,
                       HOSPITALNAME AS HospitalName,
                       DIAGNOSIS AS Diagnosis,
                       RECORDDATE AS RecordDate,
                       NEXTDUEDATE AS NextDueDate,
                       ATTACHMENTURL AS AttachmentUrl
                FROM MED_HEALTHRECORDS
                WHERE CATID = :CatID
                ORDER BY RECORDDATE DESC NULLS LAST";

            return await QueryAsync(sql, new { CatID = catId });
        }

        public async Task<MedHealthRecord?> GetById(string id)
        {
            const string sql = @"
                SELECT RECORDID AS RecordID,
                       CATID AS CatID,
                       RECORDTYPE AS RecordType,
                       HOSPITALNAME AS HospitalName,
                       DIAGNOSIS AS Diagnosis,
                       RECORDDATE AS RecordDate,
                       NEXTDUEDATE AS NextDueDate,
                       ATTACHMENTURL AS AttachmentUrl
                FROM MED_HEALTHRECORDS
                WHERE RECORDID = :RecordID";

            return await QuerySingleAsync(sql, new { RecordID = id });
        }

        public async Task<int> Create(MedHealthRecord record)
        {
            record.RecordID = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO MED_HEALTHRECORDS (RECORDID, CATID, RECORDTYPE, HOSPITALNAME, DIAGNOSIS,
                                               RECORDDATE, NEXTDUEDATE, ATTACHMENTURL)
                VALUES (:RecordID, :CatID, :RecordType, :HospitalName, :Diagnosis,
                        :RecordDate, :NextDueDate, :AttachmentUrl)";

            return await ExecuteAsync(sql, new
            {
                record.RecordID,
                record.CatID,
                record.RecordType,
                record.HospitalName,
                record.Diagnosis,
                record.RecordDate,
                record.NextDueDate,
                record.AttachmentUrl
            });
        }

        public async Task<int> Update(MedHealthRecord record)
        {
            const string sql = @"
                UPDATE MED_HEALTHRECORDS
                SET CATID = :CatID,
                    RECORDTYPE = :RecordType,
                    HOSPITALNAME = :HospitalName,
                    DIAGNOSIS = :Diagnosis,
                    RECORDDATE = :RecordDate,
                    NEXTDUEDATE = :NextDueDate,
                    ATTACHMENTURL = :AttachmentUrl
                WHERE RECORDID = :RecordID";

            return await ExecuteAsync(sql, new
            {
                record.CatID,
                record.RecordType,
                record.HospitalName,
                record.Diagnosis,
                record.RecordDate,
                record.NextDueDate,
                record.AttachmentUrl,
                record.RecordID
            });
        }

        public async Task<int> Delete(string id)
        {
            const string sql = @"DELETE FROM MED_HEALTHRECORDS WHERE RECORDID = :RecordID";

            return await ExecuteAsync(sql, new { RecordID = id });
        }
    }
}
