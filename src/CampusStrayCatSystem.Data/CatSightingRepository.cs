using System.Text;
using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    public class CatSightingRepository : BaseRepository<CatSighting>, ICatSightingRepository
    {
        private const string SelectColumns = @"
            SELECT SIGHTINGID AS SightingID,
                   CATID AS CatID,
                   USERID AS UserID,
                   AREAID AS AreaID,
                   LONGITUDE AS Longitude,
                   LATITUDE AS Latitude,
                   PHOTOURL AS PhotoUrl,
                   SIGHTINGTIME AS SightingTime,
                   REMARK AS Remark
            FROM CAT_SIGHTINGS";

        public CatSightingRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<CatSighting>> GetAllAsync(
            string? catId = null,
            string? areaId = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var sql = new StringBuilder(SelectColumns);
            sql.AppendLine(" WHERE 1 = 1");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(catId))
            {
                sql.AppendLine(" AND CATID = :CatID");
                parameters.Add("CatID", catId.Trim());
            }

            if (!string.IsNullOrWhiteSpace(areaId))
            {
                sql.AppendLine(" AND AREAID = :AreaID");
                parameters.Add("AreaID", areaId.Trim());
            }

            if (from.HasValue)
            {
                sql.AppendLine(" AND SIGHTINGTIME >= :FromTime");
                parameters.Add("FromTime", from.Value);
            }

            if (to.HasValue)
            {
                sql.AppendLine(" AND SIGHTINGTIME <= :ToTime");
                parameters.Add("ToTime", to.Value);
            }

            sql.AppendLine(" ORDER BY SIGHTINGTIME DESC NULLS LAST");
            return await QueryAsync(sql.ToString(), parameters);
        }

        public async Task<CatSighting?> GetByIdAsync(string id)
        {
            const string sql = SelectColumns + " WHERE SIGHTINGID = :SightingID";
            return await QuerySingleAsync(sql, new { SightingID = id });
        }

        public async Task<IEnumerable<CatSighting>> GetRecentByCatAsync(string catId, int limit)
        {
            const string sql = @"
                SELECT *
                FROM (
                    SELECT SIGHTINGID AS SightingID,
                           CATID AS CatID,
                           USERID AS UserID,
                           AREAID AS AreaID,
                           LONGITUDE AS Longitude,
                           LATITUDE AS Latitude,
                           PHOTOURL AS PhotoUrl,
                           SIGHTINGTIME AS SightingTime,
                           REMARK AS Remark
                    FROM CAT_SIGHTINGS
                    WHERE CATID = :CatID
                    ORDER BY SIGHTINGTIME DESC NULLS LAST
                )
                WHERE ROWNUM <= :ResultLimit";

            return await QueryAsync(sql, new { CatID = catId, ResultLimit = limit });
        }

        public async Task<bool> CatExistsAsync(string catId)
        {
            const string sql = "SELECT COUNT(1) FROM CAT_CATS WHERE CATID = :CatID";
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { CatID = catId }) > 0;
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            const string sql = "SELECT COUNT(1) FROM SYS_USERS WHERE USERID = :UserID";
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { UserID = userId }) > 0;
        }

        public async Task<bool> HasReferencesAsync(string id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM CAT_MISSINGALERTS
                WHERE LASTSIGHTINGID = :SightingID";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { SightingID = id }) > 0;
        }

        public async Task<int> CreateAsync(CatSighting sighting)
        {
            const string sql = @"
                INSERT INTO CAT_SIGHTINGS
                    (SIGHTINGID, CATID, USERID, AREAID, LONGITUDE, LATITUDE,
                     PHOTOURL, SIGHTINGTIME, REMARK)
                VALUES
                    (:SightingID, :CatID, :UserID, :AreaID, :Longitude, :Latitude,
                     :PhotoUrl, :SightingTime, :Remark)";

            return await ExecuteAsync(sql, sighting);
        }

        public async Task<int> UpdateAsync(CatSighting sighting)
        {
            const string sql = @"
                UPDATE CAT_SIGHTINGS
                SET CATID = :CatID,
                    USERID = :UserID,
                    AREAID = :AreaID,
                    LONGITUDE = :Longitude,
                    LATITUDE = :Latitude,
                    PHOTOURL = :PhotoUrl,
                    SIGHTINGTIME = :SightingTime,
                    REMARK = :Remark
                WHERE SIGHTINGID = :SightingID";

            return await ExecuteAsync(sql, sighting);
        }

        public async Task<int> DeleteAsync(string id)
        {
            const string sql = "DELETE FROM CAT_SIGHTINGS WHERE SIGHTINGID = :SightingID";
            return await ExecuteAsync(sql, new { SightingID = id });
        }
    }
}
