using CampusStrayCatSystem.Core;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace CampusStrayCatSystem.Tests {
    internal sealed class FakeCatRepository : ICatRepository {
        public CatSummary? Cat { get; set; }
        public bool ExistsResult { get; set; }

        public Task<bool> Exists(string catId) => Task.FromResult(ExistsResult);
        public Task<CatSummary?> GetByIdAsync(string catId) => Task.FromResult(Cat);
        public Task<IEnumerable<CatSummary>> GetAllAsync(string? mainAreaId = null,
                                                         string? lifeStatus = null,
                                                         string? archiveStatus = null) =>
            throw new NotSupportedException();
        public Task<CatSummary?> CreateAsync(Cat cat) => throw new NotSupportedException();
        public Task<int> UpdateAsync(Cat cat) => throw new NotSupportedException();
        public Task<int> ArchiveAsync(string catId) => throw new NotSupportedException();
    }

    internal sealed class FakeUserRepository : IUserRepository {
        public bool ExistsResult { get; set; } = true;

        public Task<bool> Exists(string userId) => Task.FromResult(ExistsResult);
    }

    internal sealed class FakeCatPhotoRepository : ICatPhotoRepository {
        public List<CatPhoto> Photos { get; } = [];
        public CatPhoto? Photo { get; set; }
        public CatPhotoFeatureData? FeatureData { get; set; }
        public CatPhotoCreateResult CreateResult { get; set; } = new() {
            Status = CatPhotoMutationStatus.Success};
        public Exception? CreateException { get; set; }
        public CatPhotoMutationStatus SetPrimaryStatus { get; set; } = CatPhotoMutationStatus.Success;
        public CatPhotoMutationStatus DeleteStatus { get; set; } = CatPhotoMutationStatus.Success;
        public Exception? DeleteException { get; set; }

        public Task<IEnumerable<CatPhoto>> GetByCatIDAsync(string catID) =>
            Task.FromResult<IEnumerable<CatPhoto>>(Photos);
        public Task<CatPhoto?> GetByIDAsync(string catID, string photoID) => Task.FromResult(Photo);
        public Task<CatPhotoFeatureData?> GetFeatureAsync(string catID, string photoID) =>
            Task.FromResult(FeatureData);

        public Task<CatPhotoCreateResult> CreateAsync(CatPhoto photo, int requestedPrimary) {
            if (CreateException != null) { throw CreateException;}
            if (CreateResult.Photo == null && CreateResult.Status == CatPhotoMutationStatus.Success) {
                CreateResult.Photo = photo;}
            return Task.FromResult(CreateResult);}

        public Task<CatPhotoMutationStatus> SetPrimaryAsync(string catID, string photoID) =>
            Task.FromResult(SetPrimaryStatus);

        public Task<CatPhotoMutationStatus> DeleteAsync(string catID, string photoID) {
            if (DeleteException != null) { throw DeleteException;}
            return Task.FromResult(DeleteStatus);}
    }

    internal sealed class FakeCatPhotoFileStorage : ICatPhotoFileStorage {
        public CatPhotoFileSaveResult SaveResult { get; set; } = new() {
            Status = CatPhotoFileSaveStatus.Success,
            PhotoUrl = "/uploads/cats/test-cat/test-photo.png"};
        public CatPhotoStagedDeletion StagedDeletion { get; set; } = new() {
            FileExisted = true,
            OriginalPath = "/test/original.png",
            StagedPath = "/test/staged.png"};
        public bool SaveCalled { get; private set; }
        public bool DeleteCalled { get; private set; }
        public bool StageCalled { get; private set; }
        public bool RestoreCalled { get; private set; }
        public bool PurgeCalled { get; private set; }

        public Task<CatPhotoFileSaveResult> SaveAsync(string catID,
                                                      string photoID,
                                                      string fileName,
                                                      string contentType,
                                                      long length,
                                                      Stream content,
                                                      CancellationToken cancellationToken = default) {
            SaveCalled = true;
            return Task.FromResult(SaveResult);}

        public Task DeleteIfExistsAsync(string photoUrl) {
            DeleteCalled = true;
            return Task.CompletedTask;}

        public Task<CatPhotoStagedDeletion> StageDeleteAsync(string photoUrl) {
            StageCalled = true;
            return Task.FromResult(StagedDeletion);}

        public Task RestoreAsync(CatPhotoStagedDeletion deletion) {
            RestoreCalled = true;
            return Task.CompletedTask;}

        public Task PurgeAsync(CatPhotoStagedDeletion deletion) {
            PurgeCalled = true;
            return Task.CompletedTask;}
    }

    internal sealed class TestWebHostEnvironment : IWebHostEnvironment {
        public string ApplicationName { get; set; } = "CampusStrayCatSystem.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
