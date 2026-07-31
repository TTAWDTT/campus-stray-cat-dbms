using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 支出记录数据访问实现，对应数据库表 FUND_FINANCERECORDS
    public class FundExpenseRecordRepository : BaseRepository<FundExpenseRecord>, IFundExpenseRecordRepository
    {
        public FundExpenseRecordRepository(IConfiguration configuration) : base(configuration) { }

        // 获取所有支出记录，按公示时间倒序
        public async Task<IEnumerable<FundExpenseRecord>> GetAll()
        {
            const string sql = @"
                SELECT FINANCEID AS FinanceID,
                       PROJECTID AS ProjectID,
                       AMOUNT AS Amount,
                       AUDITUSERID AS AuditUserID,
                       AUDITSTATUS AS AuditStatus,
                       PUBLICTIME AS PublicTime
                FROM FUND_FINANCERECORDS
                ORDER BY PUBLICTIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        // 按支出 ID 获取单条支出记录
        public async Task<FundExpenseRecord?> GetById(string financeId)
        {
            const string sql = @"
                SELECT FINANCEID AS FinanceID,
                       PROJECTID AS ProjectID,
                       AMOUNT AS Amount,
                       AUDITUSERID AS AuditUserID,
                       AUDITSTATUS AS AuditStatus,
                       PUBLICTIME AS PublicTime
                FROM FUND_FINANCERECORDS
                WHERE FINANCEID = :FinanceID";

            return await QuerySingleAsync(sql, new { FinanceID = financeId });
        }

        // 按项目查询支出记录
        public async Task<IEnumerable<FundExpenseRecord>> GetByProject(string projectId)
        {
            const string sql = @"
                SELECT FINANCEID AS FinanceID,
                       PROJECTID AS ProjectID,
                       AMOUNT AS Amount,
                       AUDITUSERID AS AuditUserID,
                       AUDITSTATUS AS AuditStatus,
                       PUBLICTIME AS PublicTime
                FROM FUND_FINANCERECORDS
                WHERE PROJECTID = :ProjectID
                ORDER BY PUBLICTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { ProjectID = projectId });
        }

        // 按项目查询已审核通过的支出记录，用于财务公示
        public async Task<IEnumerable<FundExpenseRecord>> GetApprovedExpensesByProject(string projectId)
        {
            const string sql = @"
                SELECT FINANCEID AS FinanceID,
                       PROJECTID AS ProjectID,
                       AMOUNT AS Amount,
                       AUDITUSERID AS AuditUserID,
                       AUDITSTATUS AS AuditStatus,
                       PUBLICTIME AS PublicTime
                FROM FUND_FINANCERECORDS
                WHERE PROJECTID = :ProjectID
                  AND AUDITSTATUS = :AuditStatus
                ORDER BY PUBLICTIME DESC NULLS LAST";

            return await QueryAsync(sql, new
            {
                ProjectID = projectId,
                AuditStatus = AuditStatuses.Approved
            });
        }

        // 创建支出记录（默认待审核）
        public async Task<int> Create(FundExpenseRecord record)
        {
            record.FinanceID = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO FUND_FINANCERECORDS (FINANCEID, PROJECTID, AMOUNT,
                                                 AUDITUSERID, AUDITSTATUS, PUBLICTIME)
                VALUES (:FinanceID, :ProjectID, :Amount,
                        :AuditUserID, :AuditStatus, :PublicTime)";

            return await ExecuteAsync(sql, new
            {
                record.FinanceID,
                record.ProjectID,
                record.Amount,
                record.AuditUserID,
                // 若未指定审核状态，默认为待审核
                AuditStatus = string.IsNullOrWhiteSpace(record.AuditStatus)
                    ? AuditStatuses.Pending
                    : record.AuditStatus,
                record.PublicTime
            });
        }

        // 审核支出记录：更新审核状态、审核人；若审核通过则记录公示时间
        public async Task<int> Audit(string financeId, string auditUserId, string auditStatus)
        {
            const string sql = @"
                UPDATE FUND_FINANCERECORDS
                SET AUDITUSERID = :AuditUserID,
                    AUDITSTATUS = :AuditStatus,
                    PUBLICTIME = CASE WHEN :AuditStatus = :Approved THEN SYSDATE ELSE PUBLICTIME END
                WHERE FINANCEID = :FinanceID";

            return await ExecuteAsync(sql, new
            {
                AuditUserID = auditUserId,
                AuditStatus = auditStatus,
                Approved = AuditStatuses.Approved,
                FinanceID = financeId
            });
        }

        // 统计某项目已审核通过的支出总额
        public async Task<decimal> GetTotalApprovedExpenseByProject(string projectId)
        {
            const string sql = @"
                SELECT NVL(SUM(AMOUNT), 0)
                FROM FUND_FINANCERECORDS
                WHERE PROJECTID = :ProjectID
                  AND AUDITSTATUS = :AuditStatus";

            var total = await QuerySingleAsync<decimal>(sql, new
            {
                ProjectID = projectId,
                AuditStatus = AuditStatuses.Approved
            });
            return total;
        }
    }
}
