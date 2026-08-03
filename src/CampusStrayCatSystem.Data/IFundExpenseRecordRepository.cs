using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 支出记录数据访问接口，对应数据库表 FUND_FINANCERECORDS
    // 支出记录需审核通过后才计入财务公示
    public interface IFundExpenseRecordRepository
    {
        Task<IEnumerable<FundExpenseRecord>> GetAll();                                       // 获取所有支出记录
        Task<FundExpenseRecord?> GetById(string financeId);                                  // 按支出 ID 获取单条支出记录
        Task<IEnumerable<FundExpenseRecord>> GetByProject(string projectId);                 // 按项目查询支出记录
        Task<IEnumerable<FundExpenseRecord>> GetApprovedExpensesByProject(string projectId); // 按项目查询已审核通过的支出记录（用于财务公示）
        Task<int> Create(FundExpenseRecord record);                                          // 创建支出记录（默认待审核）
        Task<int> Audit(string financeId, string auditUserId, string auditStatus);           // 审核支出记录：更新审核状态和审核人
        Task<decimal> GetTotalApprovedExpenseByProject(string projectId);                    // 统计某项目已审核通过的支出总额
    }
}
