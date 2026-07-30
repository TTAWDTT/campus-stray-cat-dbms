using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    public class CatRepository : BaseRepository<object>, ICatRepository
    {
        public CatRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<bool> Exists(string catId)
        {
            const string sql = "SELECT COUNT(1) FROM CAT_CATS WHERE CATID = :CatID";
            var count = await QuerySingleAsync<int>(sql, new { CatID = catId });
            return count > 0;
        }
    }
}
