using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    // 引用存在性校验实现，统一查询各表判断外键引用是否存在
    public class ReferenceCheckRepository : BaseRepository<object>, IReferenceCheckRepository
    {
        public ReferenceCheckRepository(IConfiguration configuration) : base(configuration) { }

        // 判断志愿者 ID 在 VOL_VOLUNTEERS 表中是否存在
        public async Task<bool> VolunteerExists(string volunteerId)
        {
            const string sql = "SELECT COUNT(1) FROM VOL_VOLUNTEERS WHERE VOLUNTEERID = :VolunteerID";
            var count = await QuerySingleAsync<int>(sql, new { VolunteerID = volunteerId });
            return count > 0;
        }

        // 判断投喂点 ID 在 MAP_SERVICEPOINTS 表中是否存在
        public async Task<bool> ServicePointExists(string pointId)
        {
            const string sql = "SELECT COUNT(1) FROM MAP_SERVICEPOINTS WHERE POINTID = :PointID";
            var count = await QuerySingleAsync<int>(sql, new { PointID = pointId });
            return count > 0;
        }

        // 判断猫咪 ID 在 CAT_CATS 表中是否存在
        public async Task<bool> CatExists(string catId)
        {
            const string sql = "SELECT COUNT(1) FROM CAT_CATS WHERE CATID = :CatID";
            var count = await QuerySingleAsync<int>(sql, new { CatID = catId });
            return count > 0;
        }

        // 判断用户 ID 在 SYS_USERS 表中是否存在
        public async Task<bool> UserExists(string userId)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_USERS WHERE USERID = :UserID";
            var count = await QuerySingleAsync<int>(sql, new { UserID = userId });
            return count > 0;
        }
    }
}
