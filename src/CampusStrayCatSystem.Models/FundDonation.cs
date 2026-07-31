namespace CampusStrayCatSystem.Models
{
    // 捐赠记录实体，对应数据库表 FUND_DONATIONS
    public class FundDonation
    {
        public string DonationID { get; set; } = string.Empty;  // 捐赠记录ID（主键）
        public string ProjectID { get; set; } = string.Empty;   // 所属众筹项目 ID（外键 FUND_CROWDFUNDINGPROJECTS.PROJECTID）
        public string? DonorUserID { get; set; }                // 捐赠人 ID（外键 SYS_USERS.USERID）
        public decimal? Amount { get; set; }                    // 捐赠金额（元）
        public DateTime? PayTime { get; set; }                  // 支付时间
        public int? PublicFlag { get; set; }                    // 是否公开（1=公开，0=匿名），用于财务公示时是否展示捐赠人
    }
}
