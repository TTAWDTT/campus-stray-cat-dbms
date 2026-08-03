namespace CampusStrayCatSystem.Core {
    public interface ICatPhotoFileStorage {
        Task<CatPhotoFileSaveResult> SaveAsync(string catID,
                                               string photoID,
                                               string fileName,
                                               string contentType,
                                               long length,
                                               Stream content,
                                               CancellationToken cancellationToken = default);
        Task DeleteIfExistsAsync(string photoUrl);
        Task<CatPhotoStagedDeletion> StageDeleteAsync(string photoUrl);
        Task RestoreAsync(CatPhotoStagedDeletion deletion);
        Task PurgeAsync(CatPhotoStagedDeletion deletion);
    }
}
