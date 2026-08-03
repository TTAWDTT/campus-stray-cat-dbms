namespace CampusStrayCatSystem.Models {
    public class Cat {
        public string CatID { get; set; } = string.Empty;
        public string? CatName { get; set; }
        public string? Gender { get; set; }
        public string? Breed { get; set; }
        public string? ColorPattern { get; set; }
        public int? SterilizedFlag { get; set; }
        public int? EarTipFlag { get; set; }
        public string? PersonalityTags { get; set; }
        public string? MainAreaId { get; set; }
        public string? LifeStatus { get; set; }
        public string? ArchiveStatus { get; set; }
    }
}
