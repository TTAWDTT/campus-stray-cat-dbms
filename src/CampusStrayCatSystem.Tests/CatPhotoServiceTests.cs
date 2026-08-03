using CampusStrayCatSystem.Core;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CampusStrayCatSystem.Tests {
    public class CatPhotoServiceTests {
        private readonly FakeCatPhotoRepository _photoRepository = new();
        private readonly FakeCatPhotoFileStorage _fileStorage = new();
        private readonly FakeCatRepository _catRepository = new();
        private readonly FakeUserRepository _userRepository = new();

        [Fact] public async Task UploadAsyncRejectsArchivedCatBeforeSavingFile() {
            _catRepository.Cat = new CatSummary { CatID = "test-cat", ArchiveStatus = CatStatusCodes.ArchiveArchived };
            var service = CreateService();

            var result = await service.UploadAsync("test-cat", CreateUploadRequest(), CancellationToken.None);

            Assert.Equal(CatPhotoServiceStatus.CatArchived, result.Status);
            Assert.False(_fileStorage.SaveCalled);}

        [Fact] public async Task UploadAsyncDeletesSavedFileWhenDatabaseWriteFails() {
            _catRepository.Cat = new CatSummary { CatID = "test-cat", ArchiveStatus = CatStatusCodes.ArchivePublished };
            _photoRepository.CreateException = new InvalidOperationException("database failure");
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UploadAsync("test-cat", CreateUploadRequest(), CancellationToken.None));

            Assert.True(_fileStorage.SaveCalled);
            Assert.True(_fileStorage.DeleteCalled);}

        [Fact] public async Task DeleteAsyncRestoresFileWhenPhotoIsReferenced() {
            _photoRepository.Photo = CreatePhoto();
            _photoRepository.DeleteStatus = CatPhotoMutationStatus.PhotoReferenced;
            var service = CreateService();

            var status = await service.DeleteAsync("test-cat", "test-photo");

            Assert.Equal(CatPhotoServiceStatus.PhotoReferenced, status);
            Assert.True(_fileStorage.StageCalled);
            Assert.True(_fileStorage.RestoreCalled);
            Assert.False(_fileStorage.PurgeCalled);}

        [Fact] public async Task DeleteAsyncPurgesStagedFileAfterDatabaseSuccess() {
            _photoRepository.Photo = CreatePhoto();
            var service = CreateService();

            var status = await service.DeleteAsync("test-cat", "test-photo");

            Assert.Equal(CatPhotoServiceStatus.Success, status);
            Assert.True(_fileStorage.StageCalled);
            Assert.True(_fileStorage.PurgeCalled);
            Assert.False(_fileStorage.RestoreCalled);}

        [Fact] public async Task GetFeatureAsyncReturnsNullForMissingVector() {
            _photoRepository.FeatureData = new CatPhotoFeatureData {
                PhotoID = "test-photo",
                CatID = "test-cat"};
            var service = CreateService();

            var result = await service.GetFeatureAsync("test-cat", "test-photo");

            Assert.Equal(CatPhotoServiceStatus.Success, result.Status);
            Assert.Null(result.Value!.FeatureVector);}

        [Fact] public async Task GetFeatureAsyncParsesJsonNumberArray() {
            _photoRepository.FeatureData = new CatPhotoFeatureData {
                PhotoID = "test-photo",
                CatID = "test-cat",
                FeatureVectorJson = "[0.12,-0.34,0.56]"};
            var service = CreateService();

            var result = await service.GetFeatureAsync("test-cat", "test-photo");

            Assert.Equal(CatPhotoServiceStatus.Success, result.Status);
            Assert.NotNull(result.Value!.FeatureVector);
            Assert.Equal([0.12, -0.34, 0.56], result.Value.FeatureVector);}

        [Theory]
        [InlineData("not-json")]
        [InlineData("null")]
        [InlineData("[1e999]")]
        public async Task GetFeatureAsyncRejectsInvalidVector(string featureVectorJson) {
            _photoRepository.FeatureData = new CatPhotoFeatureData {
                PhotoID = "test-photo",
                CatID = "test-cat",
                FeatureVectorJson = featureVectorJson};
            var service = CreateService();

            var result = await service.GetFeatureAsync("test-cat", "test-photo");

            Assert.Equal(CatPhotoServiceStatus.InvalidFeature, result.Status);}

        private CatPhotoService CreateService() => new(_photoRepository,
                                                       _fileStorage,
                                                       _catRepository,
                                                       _userRepository,
                                                       NullLogger<CatPhotoService>.Instance);

        private static UploadCatPhotoRequest CreateUploadRequest() {
            var bytes = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
            var stream = new MemoryStream(bytes);
            return new UploadCatPhotoRequest {
                File = new FormFile(stream, 0, bytes.Length, "file", "cat.png") {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"},
                UploadUserID = "test-user"};}

        private static CatPhoto CreatePhoto() => new() {
            PhotoID = "test-photo",
            CatID = "test-cat",
            PhotoUrl = "/uploads/cats/test-cat/test-photo.png"};
    }
}
