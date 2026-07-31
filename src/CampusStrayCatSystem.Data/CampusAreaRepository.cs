using System.Text;
using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    public class CampusAreaRepository : BaseRepository<CampusArea>, ICampusAreaRepository
    {
        private const string SelectColumns = @"
            SELECT AREAID AS AreaID,
                   AREANAME AS AreaName,
                   CAMPUSNAME AS CampusName,
                   PARENTAREAID AS ParentAreaID,
                   AREATYPE AS AreaType,
                   RISKLEVEL AS RiskLevel,
                   GEOBOUNDARY AS GeoBoundary
            FROM MAP_CAMPUSAREAS";

        public CampusAreaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<CampusArea>> GetAllAsync(
            string? campusName = null,
            string? areaType = null,
            string? riskLevel = null)
        {
            var sql = new StringBuilder(SelectColumns);
            sql.AppendLine(" WHERE 1 = 1");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(campusName))
            {
                sql.AppendLine(" AND CAMPUSNAME = :CampusName");
                parameters.Add("CampusName", campusName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(areaType))
            {
                sql.AppendLine(" AND AREATYPE = :AreaType");
                parameters.Add("AreaType", areaType.Trim());
            }

            if (!string.IsNullOrWhiteSpace(riskLevel))
            {
                sql.AppendLine(" AND RISKLEVEL = :RiskLevel");
                parameters.Add("RiskLevel", riskLevel.Trim());
            }

            sql.AppendLine(" ORDER BY CAMPUSNAME NULLS LAST, AREANAME");
            return await QueryAsync(sql.ToString(), parameters);
        }

        public async Task<CampusArea?> GetByIdAsync(string id)
        {
            const string sql = SelectColumns + " WHERE AREAID = :AreaID";
            return await QuerySingleAsync(sql, new { AreaID = id });
        }

        public async Task<IEnumerable<CampusArea>> GetRootsAsync()
        {
            const string sql = SelectColumns + @"
                WHERE PARENTAREAID IS NULL
                ORDER BY CAMPUSNAME NULLS LAST, AREANAME";

            return await QueryAsync(sql);
        }

        public async Task<IEnumerable<CampusArea>> GetChildrenAsync(string parentAreaId)
        {
            const string sql = SelectColumns + @"
                WHERE PARENTAREAID = :ParentAreaID
                ORDER BY AREANAME";

            return await QueryAsync(sql, new { ParentAreaID = parentAreaId });
        }

        public async Task<IEnumerable<CampusAreaHierarchyItem>> GetHierarchyAsync()
        {
            const string sql = @"
                SELECT AREAID AS AreaID,
                       AREANAME AS AreaName,
                       CAMPUSNAME AS CampusName,
                       PARENTAREAID AS ParentAreaID,
                       AREATYPE AS AreaType,
                       RISKLEVEL AS RiskLevel,
                       GEOBOUNDARY AS GeoBoundary,
                       LEVEL AS HierarchyLevel
                FROM MAP_CAMPUSAREAS
                START WITH PARENTAREAID IS NULL
                CONNECT BY NOCYCLE PRIOR AREAID = PARENTAREAID
                ORDER SIBLINGS BY AREANAME";

            using var connection = CreateConnection();
            return await connection.QueryAsync<CampusAreaHierarchyItem>(sql);
        }

        public async Task<bool> HasReferencesAsync(string id)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(1) FROM CAT_CATS WHERE MAINAREAID = :AreaID) +
                    (SELECT COUNT(1) FROM MAP_SERVICEPOINTS WHERE AREAID = :AreaID) +
                    (SELECT COUNT(1) FROM CAT_SIGHTINGS WHERE AREAID = :AreaID) +
                    (SELECT COUNT(1) FROM EMERGENCY_REPORTS WHERE AREAID = :AreaID)
                FROM DUAL";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { AreaID = id }) > 0;
        }

        public async Task<int> CreateAsync(CampusArea area)
        {
            const string sql = @"
                INSERT INTO MAP_CAMPUSAREAS
                    (AREAID, AREANAME, CAMPUSNAME, PARENTAREAID, AREATYPE, RISKLEVEL, GEOBOUNDARY)
                VALUES
                    (:AreaID, :AreaName, :CampusName, :ParentAreaID, :AreaType, :RiskLevel, :GeoBoundary)";

            return await ExecuteAsync(sql, area);
        }

        public async Task<int> UpdateAsync(CampusArea area)
        {
            const string sql = @"
                UPDATE MAP_CAMPUSAREAS
                SET AREANAME = :AreaName,
                    CAMPUSNAME = :CampusName,
                    PARENTAREAID = :ParentAreaID,
                    AREATYPE = :AreaType,
                    RISKLEVEL = :RiskLevel,
                    GEOBOUNDARY = :GeoBoundary
                WHERE AREAID = :AreaID";

            return await ExecuteAsync(sql, area);
        }

        public async Task<int> DeleteAsync(string id)
        {
            const string sql = "DELETE FROM MAP_CAMPUSAREAS WHERE AREAID = :AreaID";
            return await ExecuteAsync(sql, new { AreaID = id });
        }
    }
}
