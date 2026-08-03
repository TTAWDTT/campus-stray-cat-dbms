namespace CampusStrayCatSystem.Models
{
    // 众筹项目状态集合 对应 FUND_CROWDFUNDINGPROJECTS.PROJECTSTATUS 属性
    public static class ProjectStatuses
    {
        public const string Active =    "ACTIVE";          // 进行中，接受捐赠
        public const string Completed = "COMPLETED";       // 已结束（达标或到期）
        public const string Cancelled = "CANCELLED";       // 已取消

        public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            Active, Completed, Cancelled
        };

        // 判断状态是否合法。
        public static bool IsValid(string? status) => status != null && Allowed.Contains(status);
    }
    // 众筹项目实体，对应数据库表 FUND_CROWDFUNDINGPROJECTS
    public class FundCrowdfundingProject
    {
        public string ProjectID { get; set; } = string.Empty; // 众筹项目ID（主键）
        public string? CatID { get; set; }                    // 关联猫咪 ID（外键 CAT_CATS.CATID）
        public string Title { get; set; } = string.Empty;     // 项目标题
        public decimal? TargetAmount { get; set; }            // 目标金额（元）
        public decimal? RaisedAmount { get; set; }            // 已筹金额（元），随捐赠累加，由系统在事务中维护
        public DateTime? StartTime { get; set; }              // 项目开始时间
        public DateTime? EndTime { get; set; }                // 项目结束时间
        public string? ProjectStatus { get; set; }            // 项目状态
    }
}
