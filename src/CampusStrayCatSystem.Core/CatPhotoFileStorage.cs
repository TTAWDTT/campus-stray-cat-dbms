namespace CampusStrayCatSystem.Core {
    public class CatPhotoFileStorage : ICatPhotoFileStorage {
        public const long MaximumFileSize = 10 * 1024 * 1024;

        private const int HeaderLength = 8;
        private readonly string _uploadsRoot;
        private readonly string _trashRoot;
        private readonly string _webRoot;

        public CatPhotoFileStorage(IWebHostEnvironment environment) {
            _webRoot = Path.GetFullPath(environment.WebRootPath ??
                                        Path.Combine(environment.ContentRootPath, "wwwroot"));
            _uploadsRoot = Path.GetFullPath(Path.Combine(_webRoot, "uploads", "cats"));
            _trashRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, ".cat-photo-trash"));
            Directory.CreateDirectory(_uploadsRoot);
            Directory.CreateDirectory(_trashRoot);}

        public async Task<CatPhotoFileSaveResult> SaveAsync(string catID,
                                                            string photoID,
                                                            string fileName,
                                                            string contentType,
                                                            long length,
                                                            Stream content,
                                                            CancellationToken cancellationToken = default) {
            if (!CatPhotoValidation.IsSafePathIdentifier(catID) ||
                !CatPhotoValidation.IsSafePathIdentifier(photoID)) {
                return new CatPhotoFileSaveResult { Status = CatPhotoFileSaveStatus.InvalidIdentifier };}
            if (length <= 0) {
                return new CatPhotoFileSaveResult { Status = CatPhotoFileSaveStatus.EmptyFile };}
            if (length > MaximumFileSize) {
                return new CatPhotoFileSaveResult { Status = CatPhotoFileSaveStatus.FileTooLarge };}

            var extension = GetNormalizedExtension(fileName, contentType);
            if (extension == null) {
                return new CatPhotoFileSaveResult { Status = CatPhotoFileSaveStatus.UnsupportedFormat };}

            var header = new byte[HeaderLength];
            var headerBytesRead = await ReadHeaderAsync(content, header, cancellationToken);
            if (!HasExpectedSignature(extension, header, headerBytesRead)) {
                return new CatPhotoFileSaveResult { Status = CatPhotoFileSaveStatus.UnsupportedFormat };}

            var catDirectory = GetContainedPath(_uploadsRoot, catID);
            Directory.CreateDirectory(catDirectory);
            var physicalPath = GetContainedPath(catDirectory, photoID + extension);
            var photoUrl = $"/uploads/cats/{catID}/{photoID}{extension}";
            var createdFile = false;

            try {
                await using var destination = new FileStream(physicalPath,
                                                             FileMode.CreateNew,
                                                             FileAccess.Write,
                                                             FileShare.None,
                                                             81920,
                                                             true);
                createdFile = true;
                await destination.WriteAsync(header.AsMemory(0, headerBytesRead), cancellationToken);
                var totalBytes = await CopyRemainingAsync(content,
                                                          destination,
                                                          headerBytesRead,
                                                          cancellationToken);
                if (totalBytes > MaximumFileSize) {
                    destination.Close();
                    File.Delete(physicalPath);
                    return new CatPhotoFileSaveResult { Status = CatPhotoFileSaveStatus.FileTooLarge };}

                return new CatPhotoFileSaveResult {
                    Status = CatPhotoFileSaveStatus.Success,
                    PhotoUrl = photoUrl};} catch {
                if (createdFile && File.Exists(physicalPath)) { File.Delete(physicalPath);}
                throw;}
        }

        public Task DeleteIfExistsAsync(string photoUrl) {
            var physicalPath = ResolvePhotoUrl(photoUrl);
            if (File.Exists(physicalPath)) { File.Delete(physicalPath);}
            return Task.CompletedTask;}

        public Task<CatPhotoStagedDeletion> StageDeleteAsync(string photoUrl) {
            var originalPath = ResolvePhotoUrl(photoUrl);
            if (!File.Exists(originalPath)) {
                return Task.FromResult(new CatPhotoStagedDeletion {
                    FileExisted = false,
                    OriginalPath = originalPath});}

            var stagedPath = GetContainedPath(_trashRoot,
                                              Guid.NewGuid().ToString("N") + Path.GetExtension(originalPath));
            File.Move(originalPath, stagedPath);
            return Task.FromResult(new CatPhotoStagedDeletion {
                FileExisted = true,
                OriginalPath = originalPath,
                StagedPath = stagedPath});}

        public Task RestoreAsync(CatPhotoStagedDeletion deletion) {
            if (!deletion.FileExisted || deletion.StagedPath == null) {
                return Task.CompletedTask;}
            if (!File.Exists(deletion.StagedPath)) {
                throw new FileNotFoundException("暂存的猫咪照片不存在，无法恢复。", deletion.StagedPath);}

            Directory.CreateDirectory(Path.GetDirectoryName(deletion.OriginalPath)!);
            File.Move(deletion.StagedPath, deletion.OriginalPath);
            return Task.CompletedTask;}

        public Task PurgeAsync(CatPhotoStagedDeletion deletion) {
            if (deletion.FileExisted && deletion.StagedPath != null && File.Exists(deletion.StagedPath)) {
                File.Delete(deletion.StagedPath);}
            return Task.CompletedTask;}

        private string ResolvePhotoUrl(string photoUrl) {
            const string urlPrefix = "/uploads/cats/";
            if (!photoUrl.StartsWith(urlPrefix, StringComparison.Ordinal)) {
                throw new InvalidOperationException("数据库中的照片 URL 不属于猫咪上传目录。");}

            var relativePath = photoUrl[urlPrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
            return GetContainedPath(_uploadsRoot, relativePath);}

        private static string GetContainedPath(string root, string relativePath) {
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!fullPath.StartsWith(fullRoot, comparison)) {
                throw new InvalidOperationException("照片路径超出允许的存储目录。");}

            return fullPath;}

        private static async Task<int> ReadHeaderAsync(Stream content,
                                                       byte[] header,
                                                       CancellationToken cancellationToken) {
            var totalBytesRead = 0;
            while (totalBytesRead < header.Length) {
                var bytesRead = await content.ReadAsync(header.AsMemory(totalBytesRead), cancellationToken);
                if (bytesRead == 0) { break;}
                totalBytesRead += bytesRead;}
            return totalBytesRead;}

        private static async Task<long> CopyRemainingAsync(Stream source,
                                                           Stream destination,
                                                           long initialBytes,
                                                           CancellationToken cancellationToken) {
            var buffer = new byte[81920];
            var totalBytes = initialBytes;
            while (true) {
                var bytesRead = await source.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0) { break;}
                totalBytes += bytesRead;
                if (totalBytes > MaximumFileSize) { break;}
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);}
            return totalBytes;}

        private static string? GetNormalizedExtension(string fileName, string contentType) {
            var extension = Path.GetExtension(Path.GetFileName(fileName)).ToLowerInvariant();
            if ((extension == ".jpg" || extension == ".jpeg") &&
                string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase)) {
                return ".jpg";}
            if (extension == ".png" &&
                string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase)) {
                return ".png";}
            return null;}

        private static bool HasExpectedSignature(string extension, byte[] header, int bytesRead) {
            if (extension == ".jpg") {
                return bytesRead >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff;}
            return bytesRead >= 8 &&
                   header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4e && header[3] == 0x47 &&
                   header[4] == 0x0d && header[5] == 0x0a && header[6] == 0x1a && header[7] == 0x0a;}

    }
}
