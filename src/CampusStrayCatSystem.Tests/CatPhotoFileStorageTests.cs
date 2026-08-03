using CampusStrayCatSystem.Core;

namespace CampusStrayCatSystem.Tests {
    public class CatPhotoFileStorageTests : IDisposable {
        private static readonly byte[] PngSignature = [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a];
        private readonly string _contentRoot;
        private readonly string _webRoot;
        private readonly CatPhotoFileStorage _storage;

        public CatPhotoFileStorageTests() {
            _contentRoot = Path.Combine(Path.GetTempPath(), "cat-photo-tests-" + Guid.NewGuid().ToString("N"));
            _webRoot = Path.Combine(_contentRoot, "wwwroot");
            Directory.CreateDirectory(_webRoot);
            _storage = new CatPhotoFileStorage(new TestWebHostEnvironment {
                ContentRootPath = _contentRoot,
                WebRootPath = _webRoot});}

        [Fact] public async Task SaveAsyncStoresValidPngAndReturnsRelativeUrl() {
            var bytes = CreatePngBytes(64);
            await using var stream = new MemoryStream(bytes);

            var result = await _storage.SaveAsync("test-cat",
                                                  "test-photo",
                                                  "cat.png",
                                                  "image/png",
                                                  bytes.Length,
                                                  stream);

            Assert.Equal(CatPhotoFileSaveStatus.Success, result.Status);
            Assert.Equal("/uploads/cats/test-cat/test-photo.png", result.PhotoUrl);
            var savedPath = Path.Combine(_webRoot, "uploads", "cats", "test-cat", "test-photo.png");
            Assert.Equal(bytes, await File.ReadAllBytesAsync(savedPath));}

        [Fact] public async Task SaveAsyncStoresValidJpegWithNormalizedExtension() {
            var bytes = new byte[] { 0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46 };
            await using var stream = new MemoryStream(bytes);

            var result = await _storage.SaveAsync("test-cat",
                                                  "test-photo",
                                                  "cat.jpeg",
                                                  "image/jpeg",
                                                  bytes.Length,
                                                  stream);

            Assert.Equal(CatPhotoFileSaveStatus.Success, result.Status);
            Assert.Equal("/uploads/cats/test-cat/test-photo.jpg", result.PhotoUrl);}

        [Fact] public async Task SaveAsyncRejectsEmptyFile() {
            await using var stream = new MemoryStream();

            var result = await _storage.SaveAsync("test-cat",
                                                  "test-photo",
                                                  "cat.png",
                                                  "image/png",
                                                  0,
                                                  stream);

            Assert.Equal(CatPhotoFileSaveStatus.EmptyFile, result.Status);}

        [Theory]
        [InlineData("../cat", "test-photo")]
        [InlineData("test-cat", "../photo")]
        public async Task SaveAsyncRejectsUnsafeIdentifiers(string catID, string photoID) {
            var bytes = CreatePngBytes(16);
            await using var stream = new MemoryStream(bytes);

            var result = await _storage.SaveAsync(catID,
                                                  photoID,
                                                  "cat.png",
                                                  "image/png",
                                                  bytes.Length,
                                                  stream);

            Assert.Equal(CatPhotoFileSaveStatus.InvalidIdentifier, result.Status);}

        [Fact] public async Task SaveAsyncRejectsMismatchedSignature() {
            var bytes = new byte[32];
            await using var stream = new MemoryStream(bytes);

            var result = await _storage.SaveAsync("test-cat",
                                                  "test-photo",
                                                  "cat.png",
                                                  "image/png",
                                                  bytes.Length,
                                                  stream);

            Assert.Equal(CatPhotoFileSaveStatus.UnsupportedFormat, result.Status);}

        [Fact] public async Task SaveAsyncCountsStreamBytesBeyondDeclaredLength() {
            var bytes = CreatePngBytes((int)CatPhotoFileStorage.MaximumFileSize + 1);
            await using var stream = new MemoryStream(bytes);

            var result = await _storage.SaveAsync("test-cat",
                                                  "test-photo",
                                                  "cat.png",
                                                  "image/png",
                                                  CatPhotoFileStorage.MaximumFileSize,
                                                  stream);

            Assert.Equal(CatPhotoFileSaveStatus.FileTooLarge, result.Status);
            Assert.False(File.Exists(Path.Combine(_webRoot,
                                                  "uploads",
                                                  "cats",
                                                  "test-cat",
                                                  "test-photo.png")));}

        [Fact] public async Task SaveAsyncAcceptsFileAtExactSizeLimit() {
            var bytes = CreatePngBytes((int)CatPhotoFileStorage.MaximumFileSize);
            await using var stream = new MemoryStream(bytes);

            var result = await _storage.SaveAsync("test-cat",
                                                  "test-photo",
                                                  "cat.png",
                                                  "image/png",
                                                  bytes.Length,
                                                  stream);

            Assert.Equal(CatPhotoFileSaveStatus.Success, result.Status);
            Assert.True(File.Exists(Path.Combine(_webRoot,
                                                 "uploads",
                                                 "cats",
                                                 "test-cat",
                                                 "test-photo.png")));}

        [Fact] public async Task StageDeleteCanRestoreAndThenPurgeFile() {
            var bytes = CreatePngBytes(32);
            await using var stream = new MemoryStream(bytes);
            var saveResult = await _storage.SaveAsync("test-cat",
                                                      "test-photo",
                                                      "cat.png",
                                                      "image/png",
                                                      bytes.Length,
                                                      stream);

            var firstDeletion = await _storage.StageDeleteAsync(saveResult.PhotoUrl!);
            Assert.True(firstDeletion.FileExisted);
            Assert.False(File.Exists(firstDeletion.OriginalPath));
            await _storage.RestoreAsync(firstDeletion);
            Assert.True(File.Exists(firstDeletion.OriginalPath));

            var secondDeletion = await _storage.StageDeleteAsync(saveResult.PhotoUrl!);
            await _storage.PurgeAsync(secondDeletion);
            Assert.False(File.Exists(secondDeletion.StagedPath));}

        public void Dispose() {
            if (Directory.Exists(_contentRoot)) { Directory.Delete(_contentRoot, true);}}

        private static byte[] CreatePngBytes(int length) {
            var bytes = new byte[length];
            PngSignature.CopyTo(bytes, 0);
            return bytes;}
    }
}
