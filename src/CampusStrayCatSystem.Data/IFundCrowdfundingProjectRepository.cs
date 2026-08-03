using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 众筹项目数据访问接口，对应数据库表 FUND_CROWDFUNDINGPROJECTS
    public interface IFundCrowdfundingProjectRepository
    {
        Task<IEnumerable<FundCrowdfundingProject>> GetAll();                    // 获取所有众筹项目
        Task<FundCrowdfundingProject?> GetById(string projectId);               // 按项目 ID 获取单个众筹项目
        Task<IEnumerable<FundCrowdfundingProject>> GetByStatus(string status);  // 按状态筛选众筹项目（如查看所有进行中的项目）
        Task<IEnumerable<FundCrowdfundingProject>> GetByCat(string catId);      // 按猫咪查询众筹项目
        Task<int> Create(FundCrowdfundingProject project);                      // 创建众筹项目
        Task<int> Update(FundCrowdfundingProject project);                      // 更新众筹项目基本信息
        Task<int> UpdateStatus(string projectId, string status);                // 更新项目状态（如发布、结束）
        Task<bool> Exists(string projectId);                                    // 判断项目是否存在
    }
}
