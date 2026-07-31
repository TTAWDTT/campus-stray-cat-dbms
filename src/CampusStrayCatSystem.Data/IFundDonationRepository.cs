using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 捐赠数据访问接口，对应数据库表 FUND_DONATIONS
    // 记录捐赠时需在同一事务中把金额累加到项目的 RAISEDAMOUNT
    public interface IFundDonationRepository
    {
        Task<IEnumerable<FundDonation>> GetAll();                       // 获取所有捐赠记录
        Task<FundDonation?> GetById(string donationId);                 // 按捐赠 ID 获取单条捐赠记录
        Task<IEnumerable<FundDonation>> GetByProject(string projectId); // 按项目 ID 查询捐赠记录（用于财务公示）
        Task<IEnumerable<FundDonation>> GetByDonor(string donorUserId); // 按捐赠人查询其捐赠记录
        Task CreateWithRaisedUpdate(FundDonation donation);             // 记录捐赠（事务）：1) 插入捐赠记录；2) 累加项目已筹金额。任一步失败则回滚
        Task<decimal> GetTotalDonationByProject(string projectId);      // 统计某项目的捐赠总金额
        Task<int> GetDonationCountByProject(string projectId);          // 统计某项目的捐赠笔数
    }
}
