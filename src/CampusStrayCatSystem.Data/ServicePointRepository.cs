using System.Text;
using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    public class ServicePointRepository : BaseRepository<ServicePoint>, IServicePointRepository
    {
        private const string SelectColumns = @"
            SELECT POINTID AS PointID,
                   AREAID AS AreaID,
                   POINTNAME AS PointName,
                   POINTTYPE AS PointType,
                   LONGITUDE AS Longitude,
                   LATITUDE AS Latitude,
                   FACILITYSTATUS AS FacilityStatus,
                   DEPLOYTIME AS DeployTime
            FROM MAP_SERVICEPOINTS";

        public ServicePointRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<ServicePoint>> GetAllAsync(
            string? areaId = null,
            string? pointType = null,
            string? facilityStatus = null)
        {
            var sql = new StringBuilder(SelectColumns);
            sql.AppendLine(" WHERE 1 = 1");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(areaId))
            {
                sql.AppendLine(" AND AREAID = :AreaID");
                parameters.Add("AreaID", areaId.Trim());
            }

            if (!string.IsNullOrWhiteSpace(pointType))
            {
                sql.AppendLine(" AND POINTTYPE = :PointType");
                parameters.Add("PointType", pointType.Trim());
            }

            if (!string.IsNullOrWhiteSpace(facilityStatus))
            {
                sql.AppendLine(" AND FACILITYSTATUS = :FacilityStatus");
                parameters.Add("FacilityStatus", facilityStatus.Trim());
            }

            sql.AppendLine(" ORDER BY DEPLOYTIME DESC NULLS LAST, POINTNAME");
            return await QueryAsync(sql.ToString(), parameters);
        }

        public async Task<ServicePoint?> GetByIdAsync(string id)
        {
            const string sql = SelectColumns + " WHERE POINTID = :PointID";
            return await QuerySingleAsync(sql, new { PointID = id });
        }

        public async Task<bool> HasReferencesAsync(string id)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(1) FROM VOL_SHIFTS WHERE POINTID = :PointID) +
                    (SELECT COUNT(1) FROM NEST_MAINTENANCERECORDS WHERE POINTID = :PointID)
                FROM DUAL";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { PointID = id }) > 0;
        }

        public async Task<int> CreateAsync(ServicePoint point)
        {
            const string sql = @"
                INSERT INTO MAP_SERVICEPOINTS
                    (POINTID, AREAID, POINTNAME, POINTTYPE, LONGITUDE, LATITUDE, FACILITYSTATUS, DEPLOYTIME)
                VALUES
                    (:PointID, :AreaID, :PointName, :PointType, :Longitude, :Latitude, :FacilityStatus, :DeployTime)";

            return await ExecuteAsync(sql, point);
        }

        public async Task<int> UpdateAsync(ServicePoint point)
        {
            const string sql = @"
                UPDATE MAP_SERVICEPOINTS
                SET AREAID = :AreaID,
                    POINTNAME = :PointName,
                    POINTTYPE = :PointType,
                    LONGITUDE = :Longitude,
                    LATITUDE = :Latitude,
                    FACILITYSTATUS = :FacilityStatus,
                    DEPLOYTIME = :DeployTime
                WHERE POINTID = :PointID";

            return await ExecuteAsync(sql, point);
        }

        public async Task<int> DeleteAsync(string id)
        {
            const string sql = "DELETE FROM MAP_SERVICEPOINTS WHERE POINTID = :PointID";
            return await ExecuteAsync(sql, new { PointID = id });
        }
    }
}
