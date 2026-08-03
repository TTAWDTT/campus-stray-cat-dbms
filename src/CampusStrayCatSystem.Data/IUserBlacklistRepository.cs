using System.Collections.Generic;
using System.Threading.Tasks;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Models.DTOs;

namespace CampusStrayCatSystem.Data
{
    public interface IUserBlacklistRepository
    {
        Task<IEnumerable<UserBlacklist>> GetAllAsync(
            string userId = null,
            string status = null,
            int page = 1,
            int pageSize = 20
        );

        Task<UserBlacklist> GetByIdAsync(string blacklistId);

        Task AddAsync(UserBlacklist record);

        Task ReleaseAsync(string blacklistId, string releasedBy);

        Task<bool> HasActiveBlacklistAsync(string userId);

        Task<BlacklistStatusDto> GetActiveStatusByUserIdAsync(string userId);

        Task<int> GetTotalCountAsync(string userId = null, string status = null);
    }
}