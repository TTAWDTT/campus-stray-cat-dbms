using System.Text.Json;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Core {
    public class CatPhotoService {
        private readonly ICatPhotoRepository _catPhotoRepository;
        private readonly ICatPhotoFileStorage _fileStorage;
        private readonly ICatRepository _catRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CatPhotoService> _logger;

        public CatPhotoService(ICatPhotoRepository catPhotoRepository,
                               ICatPhotoFileStorage fileStorage,
                               ICatRepository catRepository,
                               IUserRepository userRepository,
                               ILogger<CatPhotoService> logger) {
            _catPhotoRepository = catPhotoRepository;
            _fileStorage = fileStorage;
            _catRepository = catRepository;
            _userRepository = userRepository;
            _logger = logger;}

        public async Task<CatPhotoServiceResult<IEnumerable<CatPhoto>>> GetAllAsync(string catID) {
            if (!CatPhotoValidation.IsSafePathIdentifier(catID)) {
                return Result<IEnumerable<CatPhoto>>(CatPhotoServiceStatus.InvalidIdentifier);}
            if (!await _catRepository.Exists(catID)) {
                return Result<IEnumerable<CatPhoto>>(CatPhotoServiceStatus.CatNotFound);}

            var photos = await _catPhotoRepository.GetByCatIDAsync(catID);
            return Result(CatPhotoServiceStatus.Success, photos);}

        public async Task<CatPhotoServiceResult<CatPhoto>> GetByIDAsync(string catID, string photoID) {
            if (!AreSafePathIdentifiers(catID, photoID)) {
                return Result<CatPhoto>(CatPhotoServiceStatus.InvalidIdentifier);}

            var photo = await _catPhotoRepository.GetByIDAsync(catID, photoID);
            return photo == null
                ? Result<CatPhoto>(CatPhotoServiceStatus.PhotoNotFound)
                : Result(CatPhotoServiceStatus.Success, photo);}

        public async Task<CatPhotoServiceResult<CatPhoto>> UploadAsync(string catID,
                                                                       UploadCatPhotoRequest request,
                                                                       CancellationToken cancellationToken) {
            if (!CatPhotoValidation.IsSafePathIdentifier(catID)) {
                return Result<CatPhoto>(CatPhotoServiceStatus.InvalidIdentifier);}
            if (!CatPhotoValidation.IsValidDatabaseID(request.UploadUserID)) {
                return Result<CatPhoto>(CatPhotoServiceStatus.InvalidUploadUserID);}
            if (request.IsPrimary is < 0 or > 1) {
                return Result<CatPhoto>(CatPhotoServiceStatus.InvalidPrimaryFlag);}
            if (request.File == null || request.File.Length == 0) {
                return Result<CatPhoto>(CatPhotoServiceStatus.EmptyFile);}

            var cat = await _catRepository.GetByIdAsync(catID);
            if (cat == null) {
                return Result<CatPhoto>(CatPhotoServiceStatus.CatNotFound);}
            if (CatStatusCodes.NormalizeArchiveStatus(cat.ArchiveStatus) == CatStatusCodes.ArchiveArchived) {
                return Result<CatPhoto>(CatPhotoServiceStatus.CatArchived);}
            if (!await _userRepository.Exists(request.UploadUserID!)) {
                return Result<CatPhoto>(CatPhotoServiceStatus.UploadUserNotFound);}

            var photoID = Guid.NewGuid().ToString();
            await using var content = request.File.OpenReadStream();
            var fileResult = await _fileStorage.SaveAsync(catID,
                                                          photoID,
                                                          request.File.FileName,
                                                          request.File.ContentType,
                                                          request.File.Length,
                                                          content,
                                                          cancellationToken);
            if (fileResult.Status != CatPhotoFileSaveStatus.Success) {
                return Result<CatPhoto>(MapFileStatus(fileResult.Status));}

            var photo = new CatPhoto {
                PhotoID = photoID,
                CatID = catID,
                PhotoUrl = fileResult.PhotoUrl!,
                UploadUserID = request.UploadUserID,
                UploadTime = DateTime.UtcNow,
                IsPrimary = request.IsPrimary};

            CatPhotoCreateResult createResult;
            try {
                createResult = await _catPhotoRepository.CreateAsync(photo, request.IsPrimary);} catch (Exception databaseException) {
                await DeleteSavedFileAfterFailureAsync(photo.PhotoUrl, photo.PhotoID, databaseException);
                throw;}

            if (createResult.Status != CatPhotoMutationStatus.Success || createResult.Photo == null) {
                await DeleteSavedFileAfterFailureAsync(photo.PhotoUrl, photo.PhotoID);
                return Result<CatPhoto>(MapMutationStatus(createResult.Status));}

            return Result(CatPhotoServiceStatus.Success, createResult.Photo);}

        public async Task<CatPhotoServiceStatus> SetPrimaryAsync(string catID, string photoID) {
            if (!AreSafePathIdentifiers(catID, photoID)) {
                return CatPhotoServiceStatus.InvalidIdentifier;}

            var status = await _catPhotoRepository.SetPrimaryAsync(catID, photoID);
            return MapMutationStatus(status);}

        public async Task<CatPhotoServiceResult<CatPhotoFeatureResponse>> GetFeatureAsync(string catID,
                                                                                          string photoID) {
            if (!AreSafePathIdentifiers(catID, photoID)) {
                return Result<CatPhotoFeatureResponse>(CatPhotoServiceStatus.InvalidIdentifier);}

            var featureData = await _catPhotoRepository.GetFeatureAsync(catID, photoID);
            if (featureData == null) {
                return Result<CatPhotoFeatureResponse>(CatPhotoServiceStatus.PhotoNotFound);}

            double[]? featureVector = null;
            if (featureData.FeatureVectorJson != null) {
                try {
                    featureVector = JsonSerializer.Deserialize<double[]>(featureData.FeatureVectorJson);
                    if (featureVector == null || featureVector.Any(value => !double.IsFinite(value))) {
                        return Result<CatPhotoFeatureResponse>(CatPhotoServiceStatus.InvalidFeature);}
                } catch (JsonException) {
                    return Result<CatPhotoFeatureResponse>(CatPhotoServiceStatus.InvalidFeature);}
            }

            return Result(CatPhotoServiceStatus.Success, new CatPhotoFeatureResponse {
                PhotoID = featureData.PhotoID,
                CatID = featureData.CatID,
                FeatureVector = featureVector});}

        public async Task<CatPhotoServiceStatus> DeleteAsync(string catID, string photoID) {
            if (!AreSafePathIdentifiers(catID, photoID)) {
                return CatPhotoServiceStatus.InvalidIdentifier;}

            var photo = await _catPhotoRepository.GetByIDAsync(catID, photoID);
            if (photo == null) {
                return CatPhotoServiceStatus.PhotoNotFound;}

            var stagedDeletion = await _fileStorage.StageDeleteAsync(photo.PhotoUrl);
            CatPhotoMutationStatus mutationStatus;
            try {
                mutationStatus = await _catPhotoRepository.DeleteAsync(catID, photoID);} catch (Exception databaseException) {
                await RestoreAfterFailureAsync(stagedDeletion, photoID, databaseException);
                throw;}

            if (mutationStatus != CatPhotoMutationStatus.Success) {
                await RestoreAfterFailureAsync(stagedDeletion, photoID);
                return MapMutationStatus(mutationStatus);}

            try {
                await _fileStorage.PurgeAsync(stagedDeletion);} catch (Exception exception) {
                _logger.LogWarning(exception,
                                   "猫咪照片 {PhotoID} 已从数据库删除，但回收区文件清理失败。",
                                   photoID);}
            return CatPhotoServiceStatus.Success;}

        private async Task DeleteSavedFileAfterFailureAsync(string photoUrl,
                                                            string photoID,
                                                            Exception? originalException = null) {
            try {
                await _fileStorage.DeleteIfExistsAsync(photoUrl);} catch (Exception cleanupException) {
                _logger.LogCritical(cleanupException,
                                    "猫咪照片 {PhotoID} 数据库写入失败后无法清理本地文件。",
                                    photoID);
                if (originalException != null) {
                    throw new AggregateException("数据库写入和本地文件补偿清理均失败。",
                                                 originalException,
                                                 cleanupException);}
                throw;}
        }

        private async Task RestoreAfterFailureAsync(CatPhotoStagedDeletion deletion,
                                                     string photoID,
                                                     Exception? originalException = null) {
            try {
                await _fileStorage.RestoreAsync(deletion);} catch (Exception restoreException) {
                _logger.LogCritical(restoreException,
                                    "猫咪照片 {PhotoID} 数据库删除失败后无法恢复本地文件。",
                                    photoID);
                if (originalException != null) {
                    throw new AggregateException("数据库删除和本地文件补偿恢复均失败。",
                                                 originalException,
                                                 restoreException);}
                throw;}
        }

        private static bool AreSafePathIdentifiers(string catID, string photoID) =>
            CatPhotoValidation.IsSafePathIdentifier(catID) && CatPhotoValidation.IsSafePathIdentifier(photoID);

        private static CatPhotoServiceStatus MapFileStatus(CatPhotoFileSaveStatus status) => status switch {
            CatPhotoFileSaveStatus.InvalidIdentifier => CatPhotoServiceStatus.InvalidIdentifier,
            CatPhotoFileSaveStatus.EmptyFile => CatPhotoServiceStatus.EmptyFile,
            CatPhotoFileSaveStatus.FileTooLarge => CatPhotoServiceStatus.FileTooLarge,
            CatPhotoFileSaveStatus.UnsupportedFormat => CatPhotoServiceStatus.UnsupportedFormat,
            _ => throw new InvalidOperationException($"无法映射文件保存状态 {status}。")};

        private static CatPhotoServiceStatus MapMutationStatus(CatPhotoMutationStatus status) => status switch {
            CatPhotoMutationStatus.Success => CatPhotoServiceStatus.Success,
            CatPhotoMutationStatus.CatNotFound => CatPhotoServiceStatus.CatNotFound,
            CatPhotoMutationStatus.PhotoNotFound => CatPhotoServiceStatus.PhotoNotFound,
            CatPhotoMutationStatus.CatArchived => CatPhotoServiceStatus.CatArchived,
            CatPhotoMutationStatus.PhotoReferenced => CatPhotoServiceStatus.PhotoReferenced,
            _ => throw new InvalidOperationException($"无法映射照片数据库状态 {status}。")};

        private static CatPhotoServiceResult<T> Result<T>(CatPhotoServiceStatus status, T? value = default) =>
            new() { Status = status, Value = value };
    }
}
