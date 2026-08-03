using System.Data;
using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CampusStrayCatSystem.Data {
    public class CatPhotoRepository : BaseRepository<CatPhoto>, ICatPhotoRepository {
        private const string SelectClause = @"
            SELECT PHOTOID AS PhotoID,
                   CATID AS CatID,
                   PHOTOURL AS PhotoUrl,
                   UPLOADUSERID AS UploadUserID,
                   UPLOADTIME AS UploadTime,
                   NVL(ISPRIMARY, 0) AS IsPrimary
            FROM CAT_PHOTOS";

        public CatPhotoRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<CatPhoto>> GetByCatIDAsync(string catID) {
            const string sql = SelectClause + @"
                WHERE CATID = :CatID
                ORDER BY NVL(ISPRIMARY, 0) DESC,
                         UPLOADTIME DESC NULLS LAST,
                         PHOTOID";

            return await QueryAsync(sql, new { CatID = catID });}

        public async Task<CatPhoto?> GetByIDAsync(string catID, string photoID) {
            const string sql = SelectClause + @"
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";

            return await QuerySingleAsync(sql, new { CatID = catID, PhotoID = photoID });}

        public async Task<CatPhotoFeatureData?> GetFeatureAsync(string catID, string photoID) {
            const string sql = @"
                SELECT PHOTOID AS PhotoID,
                       CATID AS CatID,
                       FEATUREVECTOR AS FeatureVectorJson
                FROM CAT_PHOTOS
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";

            return await QuerySingleAsync<CatPhotoFeatureData>(sql, new { CatID = catID, PhotoID = photoID });}

        public async Task<CatPhotoCreateResult> CreateAsync(CatPhoto photo, int requestedPrimary) {
            const string countSql = "SELECT COUNT(1) FROM CAT_PHOTOS WHERE CATID = :CatID";
            const string clearPrimarySql = @"
                UPDATE CAT_PHOTOS
                SET ISPRIMARY = 0
                WHERE CATID = :CatID
                  AND ISPRIMARY = 1";
            const string insertSql = @"
                INSERT INTO CAT_PHOTOS (
                    PHOTOID, CATID, PHOTOURL, FEATUREVECTOR,
                    UPLOADUSERID, UPLOADTIME, ISPRIMARY
                ) VALUES (
                    :PhotoID, :CatID, :PhotoUrl, NULL,
                    :UploadUserID, :UploadTime, :IsPrimary
                )";
            const string selectSql = SelectClause + @"
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try {
                var cat = await LockCatAsync(connection, transaction, photo.CatID!);
                if (cat == null) {
                    transaction.Rollback();
                    return new CatPhotoCreateResult { Status = CatPhotoMutationStatus.CatNotFound };}
                if (IsArchived(cat.ArchiveStatus)) {
                    transaction.Rollback();
                    return new CatPhotoCreateResult { Status = CatPhotoMutationStatus.CatArchived };}

                var photoCount = await connection.ExecuteScalarAsync<int>(countSql, new { photo.CatID }, transaction);
                photo.IsPrimary = requestedPrimary == 1 || photoCount == 0 ? 1 : 0;
                if (photo.IsPrimary == 1) {
                    await connection.ExecuteAsync(clearPrimarySql, new { photo.CatID }, transaction);}

                await connection.ExecuteAsync(insertSql, photo, transaction);
                var createdPhoto = await connection.QueryFirstOrDefaultAsync<CatPhoto>(selectSql,
                                                                                        new { photo.CatID, photo.PhotoID },
                                                                                        transaction);
                if (createdPhoto == null) {
                    throw new InvalidOperationException("创建照片后无法读取数据库记录。");}

                transaction.Commit();
                return new CatPhotoCreateResult {
                    Status = CatPhotoMutationStatus.Success,
                    Photo = createdPhoto};} catch {
                transaction.Rollback();
                throw;}
        }

        public async Task<CatPhotoMutationStatus> SetPrimaryAsync(string catID, string photoID) {
            const string photoExistsSql = @"
                SELECT COUNT(1)
                FROM CAT_PHOTOS
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";
            const string clearPrimarySql = @"
                UPDATE CAT_PHOTOS
                SET ISPRIMARY = 0
                WHERE CATID = :CatID
                  AND ISPRIMARY = 1";
            const string setPrimarySql = @"
                UPDATE CAT_PHOTOS
                SET ISPRIMARY = 1
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try {
                var cat = await LockCatAsync(connection, transaction, catID);
                if (cat == null) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.CatNotFound;}
                if (IsArchived(cat.ArchiveStatus)) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.CatArchived;}

                var photoExists = await connection.ExecuteScalarAsync<int>(photoExistsSql,
                                                                            new { CatID = catID, PhotoID = photoID },
                                                                            transaction);
                if (photoExists == 0) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.PhotoNotFound;}

                await connection.ExecuteAsync(clearPrimarySql, new { CatID = catID }, transaction);
                var affectedRows = await connection.ExecuteAsync(setPrimarySql,
                                                                  new { CatID = catID, PhotoID = photoID },
                                                                  transaction);
                if (affectedRows == 0) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.PhotoNotFound;}

                transaction.Commit();
                return CatPhotoMutationStatus.Success;} catch {
                transaction.Rollback();
                throw;}
        }

        public async Task<CatPhotoMutationStatus> DeleteAsync(string catID, string photoID) {
            const string primarySql = @"
                SELECT NVL(ISPRIMARY, 0)
                FROM CAT_PHOTOS
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";
            const string referenceSql = @"
                SELECT COUNT(1)
                FROM CAT_MATCHRECORDS
                WHERE SOURCEPHOTOID = :PhotoID";
            const string deleteSql = @"
                DELETE FROM CAT_PHOTOS
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";
            const string replacementSql = @"
                SELECT PHOTOID
                FROM (
                    SELECT PHOTOID
                    FROM CAT_PHOTOS
                    WHERE CATID = :CatID
                    ORDER BY UPLOADTIME DESC NULLS LAST, PHOTOID
                )
                WHERE ROWNUM = 1";
            const string setPrimarySql = @"
                UPDATE CAT_PHOTOS
                SET ISPRIMARY = 1
                WHERE CATID = :CatID
                  AND PHOTOID = :PhotoID";

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try {
                var cat = await LockCatAsync(connection, transaction, catID);
                if (cat == null) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.CatNotFound;}

                var isPrimary = await connection.QueryFirstOrDefaultAsync<int?>(primarySql,
                                                                                 new { CatID = catID, PhotoID = photoID },
                                                                                 transaction);
                if (!isPrimary.HasValue) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.PhotoNotFound;}

                var referenceCount = await connection.ExecuteScalarAsync<int>(referenceSql,
                                                                                new { PhotoID = photoID },
                                                                                transaction);
                if (referenceCount > 0) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.PhotoReferenced;}

                var affectedRows = await connection.ExecuteAsync(deleteSql,
                                                                  new { CatID = catID, PhotoID = photoID },
                                                                  transaction);
                if (affectedRows == 0) {
                    transaction.Rollback();
                    return CatPhotoMutationStatus.PhotoNotFound;}

                if (isPrimary.Value == 1) {
                    var replacementPhotoID = await connection.QueryFirstOrDefaultAsync<string?>(replacementSql,
                                                                                                 new { CatID = catID },
                                                                                                 transaction);
                    if (replacementPhotoID != null) {
                        await connection.ExecuteAsync(setPrimarySql,
                                                      new { CatID = catID, PhotoID = replacementPhotoID },
                                                      transaction);}
                }

                transaction.Commit();
                return CatPhotoMutationStatus.Success;} catch (OracleException exception) when (exception.Number == 2292) {
                transaction.Rollback();
                return CatPhotoMutationStatus.PhotoReferenced;} catch {
                transaction.Rollback();
                throw;}
        }

        private static async Task<LockedCatState?> LockCatAsync(IDbConnection connection,
                                                                 IDbTransaction transaction,
                                                                 string catID) {
            const string sql = @"
                SELECT CATID AS CatID,
                       ARCHIVESTATUS AS ArchiveStatus
                FROM CAT_CATS
                WHERE CATID = :CatID
                FOR UPDATE";

            return await connection.QueryFirstOrDefaultAsync<LockedCatState>(sql, new { CatID = catID }, transaction);}

        private static bool IsArchived(string? archiveStatus) =>
            CatStatusCodes.NormalizeArchiveStatus(archiveStatus) == CatStatusCodes.ArchiveArchived;

        private sealed class LockedCatState {
            public string CatID { get; set; } = string.Empty;
            public string? ArchiveStatus { get; set; }
        }
    }
}
