namespace CampusStrayCatSystem.Models {
    public static class CatStatusCodes {
        public const string GenderUnknown = "UNKNOWN";
        public const string GenderMale = "MALE";
        public const string GenderFemale = "FEMALE";

        public const string LifeOnCampus = "ON_CAMPUS";
        public const string LifeMissing = "MISSING";
        public const string LifeAdopted = "ADOPTED";
        public const string LifeDeceased = "DECEASED";

        public const string ArchiveDraft = "DRAFT";
        public const string ArchivePublished = "PUBLISHED";
        public const string ArchiveArchived = "ARCHIVED";

        internal const string GenderPattern = "^(UNKNOWN|MALE|FEMALE)$";
        internal const string LifeStatusPattern = "^(ON_CAMPUS|MISSING|ADOPTED|DECEASED)$";
        internal const string ArchiveStatusPattern = "^(DRAFT|PUBLISHED|ARCHIVED)$";

        public static string? NormalizeGender(string? value) => Normalize(value) switch {
            "母" or "雌" => GenderFemale,
            "公" or "雄" => GenderMale,
            "未知" => GenderUnknown,
            var normalized => normalized};

        public static string? NormalizeLifeStatus(string? value) => Normalize(value) switch {
            "在校" or "ACTIVE" => LifeOnCampus,
            "失踪" => LifeMissing,
            "已领养" => LifeAdopted,
            "已死亡" => LifeDeceased,
            var normalized => normalized};

        public static string? NormalizeArchiveStatus(string? value) => Normalize(value) switch {
            "草稿" => ArchiveDraft,
            "正常" or "NORMAL" => ArchivePublished,
            "已归档" => ArchiveArchived,
            var normalized => normalized};

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }
}
