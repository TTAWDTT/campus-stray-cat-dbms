using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    // 支出记录审核状态集合 对应 FUND_FINANCERECORDS.AUDITSTATUS 属性
    public static class AuditStatuses
    {
        public const string Pending =   "PENDING";     // 待审核
        public const string Approved =  "APPROVED";    // 已通过
        public const string Rejected =  "REJECTED";    // 已驳回

        public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            Pending, Approved, Rejected
        };

        // 判断状态是否合法。
        public static bool IsValid(string? status) => status != null && Allowed.Contains(status);
    }
    // 支出记录实体，对应数据库表 FUND_FINANCERECORDS（仅记录支出，收入由 FUND_DONATIONS 跟踪）
    public class FundExpenseRecord
    {
        public string FinanceID { get; set; } = string.Empty;  // 支出记录ID（主键）
        public string ProjectID { get; set; } = string.Empty;  // 所属众筹项目 ID（外键 FUND_CROWDFUNDINGPROJECTS.PROJECTID）
        public string? RecordType { get; set; }                // 财务记录类型
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "支出金额必须大于 0。")]
        public decimal? Amount { get; set; }                   // 支出金额（元）
        public string? InvoiceUrl { get; set; }                // 发票或凭证地址
        public string? AuditUserID { get; set; }               // 审核人 ID（外键 SYS_USERS.USERID）
        public string? AuditStatus { get; set; }               // 审核状态
        public DateTime? PublicTime { get; set; }              // 公示时间
    }
}
