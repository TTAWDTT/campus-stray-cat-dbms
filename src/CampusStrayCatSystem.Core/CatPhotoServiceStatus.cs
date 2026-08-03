namespace CampusStrayCatSystem.Core {
    public enum CatPhotoServiceStatus {
        Success,
        InvalidIdentifier,
        InvalidUploadUserID,
        InvalidPrimaryFlag,
        EmptyFile,
        FileTooLarge,
        UnsupportedFormat,
        CatNotFound,
        PhotoNotFound,
        UploadUserNotFound,
        CatArchived,
        PhotoReferenced,
        InvalidFeature
    }
}
