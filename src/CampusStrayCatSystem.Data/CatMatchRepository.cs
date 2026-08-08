using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CampusStrayCatSystem.Data {
    public class CatMatchRepository : BaseRepository<CatMatchRecord>, ICatMatchRepository {
        private const string NormalizedConfirmStatusSql = @"CASE
                WHEN m.CONFIRMSTATUS IS NULL OR TRIM(m.CONFIRMSTATUS) IS NULL THEN 'PENDING'
                ELSE UPPER(TRIM(m.CONFIRMSTATUS))
            END";
        private const string NormalizedArchiveStatusSql = @"CASE UPPER(TRIM(c.ARCHIVESTATUS))
                WHEN '草稿' THEN 'DRAFT'
                WHEN '正常' THEN 'PUBLISHED'
                WHEN 'NORMAL' THEN 'PUBLISHED'
                WHEN '已归档' THEN 'ARCHIVED'
                ELSE UPPER(TRIM(c.ARCHIVESTATUS))
            END";
        private const string PrimaryPhotoJoin = @"
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
            ) primaryPhoto ON primaryPhoto.CATID = c.CATID";
        private const string SelectClause = @"
            SELECT m.MATCHID AS MatchID,
                   m.SOURCEPHOTOID AS SourcePhotoID,
                   m.CANDIDATECATID AS CandidateCatID,
                   m.SIMILARITYSCORE AS SimilarityScore,
                   m.RANKNO AS RankNo,
                   " + NormalizedConfirmStatusSql + @" AS ConfirmStatus,
                   m.CONFIRMUSERID AS ConfirmUserID,
                   sourcePhoto.PHOTOURL AS SourcePhotoUrl,
                   c.CATNAME AS CandidateCatName,
                   " + NormalizedArchiveStatusSql + @" AS CandidateArchiveStatus,
                   area.AREANAME AS CandidateAreaName,
                   primaryPhoto.PHOTOURL AS CandidatePrimaryPhotoUrl
            FROM CAT_MATCHRECORDS m
            LEFT JOIN CAT_PHOTOS sourcePhoto ON sourcePhoto.PHOTOID = m.SOURCEPHOTOID
            LEFT JOIN CAT_CATS c ON c.CATID = m.CANDIDATECATID
            LEFT JOIN MAP_CAMPUSAREAS area ON area.AREAID = c.MAINAREAID" + PrimaryPhotoJoin;

        public CatMatchRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<CatMatchRecord>> GetBySourcePhotoAsync(string catID,
                                                                              string photoID,
                                                                              string? candidateCatID,
                                                                              string? confirmStatus) {
            const string sql = SelectClause + @"
                WHERE m.SOURCEPHOTOID = :PhotoID
                  AND sourcePhoto.CATID = :CatID
                  AND (:CandidateCatID IS NULL OR m.CANDIDATECATID = :CandidateCatID)
                  AND (:ConfirmStatus IS NULL OR (" + NormalizedConfirmStatusSql + @") = :ConfirmStatus)
                ORDER BY CASE WHEN m.RANKNO IS NULL THEN 1 ELSE 0 END,
                         m.RANKNO,
                         CASE WHEN m.SIMILARITYSCORE IS NULL THEN 1 ELSE 0 END,
                         m.SIMILARITYSCORE DESC,
                         m.MATCHID";
            var normalizedStatus = string.IsNullOrWhiteSpace(confirmStatus)
                ? null
                : CatMatchStatusCodes.Normalize(confirmStatus);

            return await QueryAsync(sql, new {
                CatID = catID,
                PhotoID = photoID,
                CandidateCatID = candidateCatID,
                ConfirmStatus = normalizedStatus});}

        public async Task<CatMatchRecord?> GetByIDAsync(string matchID) {
            const string sql = SelectClause + @"
                WHERE m.MATCHID = :MatchID";

            return await QuerySingleAsync(sql, new { MatchID = matchID });}

        public async Task<CatMatchMutationStatus> ConfirmAsync(string matchID,
                                                               string confirmStatus,
                                                               string confirmUserID) {
            const string lockMatchSql = @"
                SELECT SOURCEPHOTOID AS SourcePhotoID,
                       CANDIDATECATID AS CandidateCatID
                FROM CAT_MATCHRECORDS
                WHERE MATCHID = :MatchID
                FOR UPDATE";
            const string associationSql = @"
                SELECT (SELECT COUNT(1) FROM CAT_PHOTOS WHERE PHOTOID = :SourcePhotoID) AS SourcePhotoCount,
                       (SELECT COUNT(1) FROM CAT_CATS WHERE CATID = :CandidateCatID) AS CandidateCatCount
                FROM DUAL";
            const string updateSql = @"
                UPDATE CAT_MATCHRECORDS
                SET CONFIRMSTATUS = :ConfirmStatus,
                    CONFIRMUSERID = :ConfirmUserID
                WHERE MATCHID = :MatchID";

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try {
                var match = await connection.QueryFirstOrDefaultAsync<MatchLinkState>(lockMatchSql,
                                                                                         new { MatchID = matchID },
                                                                                         transaction);
                if (match == null) {
                    transaction.Rollback();
                    return CatMatchMutationStatus.MatchNotFound;}
                if (string.IsNullOrWhiteSpace(match.SourcePhotoID) ||
                    string.IsNullOrWhiteSpace(match.CandidateCatID)) {
                    transaction.Rollback();
                    return CatMatchMutationStatus.AssociationUnavailable;}

                var association = await connection.QuerySingleAsync<AssociationState>(associationSql,
                                                                                        new {
                                                                                            match.SourcePhotoID,
                                                                                            match.CandidateCatID},
                                                                                        transaction);
                if (association.SourcePhotoCount == 0 || association.CandidateCatCount == 0) {
                    transaction.Rollback();
                    return CatMatchMutationStatus.AssociationUnavailable;}

                var affectedRows = await connection.ExecuteAsync(updateSql,
                                                                  new {
                                                                      MatchID = matchID,
                                                                      ConfirmStatus = CatMatchStatusCodes.Normalize(confirmStatus),
                                                                      ConfirmUserID = confirmUserID},
                                                                  transaction);
                if (affectedRows != 1) {
                    transaction.Rollback();
                    return CatMatchMutationStatus.MatchNotFound;}

                transaction.Commit();
                return CatMatchMutationStatus.Success;} catch (OracleException exception) when (exception.Number == 2291) {
                transaction.Rollback();
                return CatMatchMutationStatus.AssociationUnavailable;} catch {
                transaction.Rollback();
                throw;}
        }

        private sealed class MatchLinkState {
            public string? SourcePhotoID { get; set; }
            public string? CandidateCatID { get; set; }
        }

        private sealed class AssociationState {
            public int SourcePhotoCount { get; set; }
            public int CandidateCatCount { get; set; }
        }
    }
}
