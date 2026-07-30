using CampusStrayCatSystem.Models;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data {
    public class CampusAreaRepository : BaseRepository<CampusArea>, ICampusAreaRepository {
        public CampusAreaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<CampusArea>> GetAllAsync() {
            const string sql = @"
                SELECT AREAID AS AreaId,
                       AREANAME AS AreaName,
                       CAMPUSNAME AS CampusName,
                       PARENTAREAID AS ParentAreaId,
                       AREATYPE AS AreaType,
                       RISKLEVEL AS RiskLevel,
                       GEOBOUNDARY AS GeoBoundary
                FROM MAP_CAMPUSAREAS
                ORDER BY AREANAME, AREAID";

            return await QueryAsync(sql);}

        public async Task<CampusArea?> GetByIdAsync(string areaId) {
            const string sql = @"
                SELECT AREAID AS AreaId,
                       AREANAME AS AreaName,
                       CAMPUSNAME AS CampusName,
                       PARENTAREAID AS ParentAreaId,
                       AREATYPE AS AreaType,
                       RISKLEVEL AS RiskLevel,
                       GEOBOUNDARY AS GeoBoundary
                FROM MAP_CAMPUSAREAS
                WHERE AREAID = :AreaId";

            return await QuerySingleAsync(sql, new { AreaId = areaId });}
    }
}
