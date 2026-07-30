using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    public class UserRepository : BaseRepository<object>, IUserRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<bool> Exists(string userId)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_USERS WHERE USERID = :UserID";
            var count = await QuerySingleAsync<int>(sql, new { UserID = userId });
            return count > 0;
        }
    }
}
