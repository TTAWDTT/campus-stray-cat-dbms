using System.Collections.Generic;
using System.Threading.Tasks;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 用户黑名单数据访问接口
    /// </summary>
    public interface IUserBlacklistRepository
    {
        /// <summary>
        /// 获取所有黑名单记录（支持筛选）
        /// </summary>
        Task<IEnumerable<UserBlacklist>> GetAllAsync(
            string userId = null, 
            string status = null, 
            int page = 1, 
            int pageSize = 20
        );

        /// <summary>
        /// 根据ID获取黑名单详情
        /// </summary>
        Task<UserBlacklist> GetByIdAsync(string blacklistId);

        /// <summary>
        /// 加入黑名单
        /// </summary>
        Task AddAsync(UserBlacklist record);

        /// <summary>
        /// 解除黑名单
        /// </summary>
        Task ReleaseAsync(string blacklistId, string releasedBy);

        /// <summary>
        /// 检查用户是否已有有效黑名单记录
        /// </summary>
        Task<bool> HasActiveBlacklistAsync(string userId);

        /// <summary>
        /// 获取用户有效黑名单状态（供领养模块调用）
        /// </summary>
        Task<BlacklistStatusDto> GetActiveStatusByUserIdAsync(string userId);
    }
}