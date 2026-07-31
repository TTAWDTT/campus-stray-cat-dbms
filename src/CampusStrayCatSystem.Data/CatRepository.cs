using System.Text;
using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data {
    public class CatRepository : BaseRepository<CatSummary>, ICatRepository {
        private const string SelectClause = @"
            SELECT c.CATID AS CatId, c.CATNAME AS CatName, c.GENDER AS Gender, c.BREED AS Breed,
                   c.COLORPATTERN AS ColorPattern, c.STERILIZEDFLAG AS SterilizedFlag, c.EARTIPFLAG AS EarTipFlag,
                   c.PERSONALITYTAGS AS PersonalityTags, c.MAINAREAID AS MainAreaId, c.LIFESTATUS AS LifeStatus,
                   c.ARCHIVESTATUS AS ArchiveStatus, a.AREANAME AS MainAreaName, p.PHOTOURL AS PrimaryPhotoUrl
            FROM CAT_CATS c
            LEFT JOIN MAP_CAMPUSAREAS a ON a.AREAID = c.MAINAREAID
            LEFT JOIN (
                SELECT CATID, PHOTOURL
                FROM (
                    SELECT CATID,
                           PHOTOURL,
                           ROW_NUMBER() OVER (
                               PARTITION BY CATID
                               ORDER BY UPLOADTIME DESC NULLS LAST, PHOTOID
                           ) AS RowNumber
                    FROM CAT_PHOTOS
                    WHERE ISPRIMARY = 1
                )
                WHERE RowNumber = 1
            ) p ON p.CATID = c.CATID";

        public CatRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<bool> Exists(string catId) {
            const string sql = "SELECT COUNT(1) FROM CAT_CATS WHERE CATID = :CatId";
            var count = await QuerySingleAsync<int>(sql, new { CatId = catId });
            return count > 0;}

        public async Task<IEnumerable<CatSummary>> GetAllAsync(string? mainAreaId = null,
                                                               string? lifeStatus = null,
                                                               string? archiveStatus = null) {
            var sql = new StringBuilder(SelectClause);
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(mainAreaId)) {
                conditions.Add("c.MAINAREAID = :MainAreaId");
                parameters.Add("MainAreaId", mainAreaId);}

            if (!string.IsNullOrWhiteSpace(lifeStatus)) {
                conditions.Add("c.LIFESTATUS = :LifeStatus");
                parameters.Add("LifeStatus", lifeStatus);}

            if (string.IsNullOrWhiteSpace(archiveStatus)) {
                conditions.Add("(c.ARCHIVESTATUS IS NULL OR c.ARCHIVESTATUS <> :ArchivedStatus)");
                parameters.Add("ArchivedStatus", "ARCHIVED");} else {
                conditions.Add("c.ARCHIVESTATUS = :ArchiveStatus");
                parameters.Add("ArchiveStatus", archiveStatus);}

            sql.Append(" WHERE ").Append(string.Join(" AND ", conditions));
            sql.Append(" ORDER BY c.CATNAME NULLS LAST, c.CATID");

            return await QueryAsync(sql.ToString(), parameters);}

        public async Task<CatSummary?> GetByIdAsync(string catId) {
            const string sql = SelectClause + @"
                WHERE c.CATID = :CatId";

            return await QuerySingleAsync(sql, new { CatId = catId });}

        public async Task<CatSummary?> CreateAsync(Cat cat) {
            const string insertSql = @"
                INSERT INTO CAT_CATS (
                    CATID, CATNAME, GENDER, BREED, COLORPATTERN, STERILIZEDFLAG,
                    EARTIPFLAG, PERSONALITYTAGS, MAINAREAID, LIFESTATUS, ARCHIVESTATUS
                ) VALUES (
                    :CatId, :CatName, :Gender, :Breed, :ColorPattern, :SterilizedFlag,
                    :EarTipFlag, :PersonalityTags, :MainAreaId, :LifeStatus, :ArchiveStatus
                )";
            const string selectSql = SelectClause + @"
                WHERE c.CATID = :CatId";

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try {
                await connection.ExecuteAsync(insertSql, cat, transaction);
                var createdCat = await connection.QueryFirstOrDefaultAsync<CatSummary>(selectSql, new { cat.CatId }, transaction);
                if (createdCat == null) {
                    transaction.Rollback();
                    return null;}

                transaction.Commit();
                return createdCat;} catch {
                transaction.Rollback();
                throw;}
        }

        public async Task<int> UpdateAsync(Cat cat) {
            const string sql = @"
                UPDATE CAT_CATS
                SET CATNAME = :CatName,
                    GENDER = :Gender,
                    BREED = :Breed,
                    COLORPATTERN = :ColorPattern,
                    STERILIZEDFLAG = :SterilizedFlag,
                    EARTIPFLAG = :EarTipFlag,
                    PERSONALITYTAGS = :PersonalityTags,
                    MAINAREAID = :MainAreaId,
                    LIFESTATUS = :LifeStatus,
                    ARCHIVESTATUS = :ArchiveStatus
                WHERE CATID = :CatId";

            return await ExecuteAsync(sql, cat);}
    }
}
