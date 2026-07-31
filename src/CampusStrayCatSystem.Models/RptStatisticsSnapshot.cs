namespace CampusStrayCatSystem.Models
{
    // 统计快照实体，对应数据库表 RPT_STATISTICSSNAPSHOTS
    public class RptStatisticsSnapshot
    {
        public string SnapshotID { get; set; } = string.Empty;  // 主键
        public DateTime? SnapshotDate { get; set; }             // 统计快照对应的日期
        public string? MetricCode { get; set; }                 // 指标代码，如 TOTAL_DONATION（总捐赠）、TOTAL_EXPENSE（总支出）、NET_BALANCE（净余额）
        public decimal? MetricValue { get; set; }               // 指标数值
        public string? DimensionType { get; set; }              // 维度类型，如 PROJECT（按项目）、MONTH（按月）、CAT（按猫咪）
        public string? DimensionValue { get; set; }             // 维度值，如项目 ID、月份字符串、猫咪 ID
        public string? Unit { get; set; }                       // 单位，如 CNY（元）、COUNT（次）
        public DateTime? GenerateTime { get; set; }             // 快照生成时间
        public string? Remark { get; set; }                     // 备注
    }
    // 财务公示视图对象，把众筹项目的收支情况聚合为一条公示信息，供前端展示财务透明度
    public class FinancialDisclosureDto
    {
        public FundCrowdfundingProject? Project { get; set; }                                              // 项目基本信息
        public decimal? TargetAmount => Project?.TargetAmount;                                             // 目标金额
        public decimal? RaisedAmount => Project?.RaisedAmount;                                             // 已筹金额（捐赠累计）
        public decimal? TotalExpense { get; set; }                                                         // 已审核通过的支出总额
        public decimal? NetBalance => (RaisedAmount ?? 0) - (TotalExpense ?? 0);                           // 净余额 = 已筹金额 - 已通过支出
        public int DonationCount { get; set; }                                                             // 捐赠笔数
        public IEnumerable<FundDonation> Donations { get; set; } = new List<FundDonation>();               // 捐赠明细列表（公开的捐赠记录）
        public IEnumerable<FundExpenseRecord> Expenses { get; set; } = new List<FundExpenseRecord>();      // 支出明细列表（已审核通过的支出记录）
    }
}
