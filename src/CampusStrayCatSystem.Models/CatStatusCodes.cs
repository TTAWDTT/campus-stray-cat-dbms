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
    }
}
