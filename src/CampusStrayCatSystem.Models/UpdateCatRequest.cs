using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models {
    public class UpdateCatRequest : CatWriteRequest {
        private string? _gender;
        private string? _lifeStatus;
        private string? _archiveStatus;

        [Required(ErrorMessage = "性别不能为空。")]
        [StringLength(10, ErrorMessage = "性别代码不能超过 10 个字符。")]
        [RegularExpression(CatStatusCodes.GenderPattern, ErrorMessage = "性别只能是 UNKNOWN、MALE 或 FEMALE。")]
        public string? Gender { get => _gender; set => _gender = NormalizeStatus(value); }

        [Required(ErrorMessage = "绝育标志不能为空。")]
        [Range(0, 1, ErrorMessage = "绝育标志只能是 0 或 1。")]
        public int? SterilizedFlag { get; set; }

        [Required(ErrorMessage = "剪耳标志不能为空。")]
        [Range(0, 1, ErrorMessage = "剪耳标志只能是 0 或 1。")]
        public int? EarTipFlag { get; set; }

        [Required(ErrorMessage = "生活状态不能为空。")]
        [StringLength(20, ErrorMessage = "生活状态代码不能超过 20 个字符。")]
        [RegularExpression(CatStatusCodes.LifeStatusPattern, ErrorMessage = "生活状态只能是 ON_CAMPUS、MISSING、ADOPTED 或 DECEASED。")]
        public string? LifeStatus { get => _lifeStatus; set => _lifeStatus = NormalizeStatus(value); }

        [Required(ErrorMessage = "档案状态不能为空。")]
        [StringLength(20, ErrorMessage = "档案状态代码不能超过 20 个字符。")]
        [RegularExpression(CatStatusCodes.ArchiveStatusPattern, ErrorMessage = "档案状态只能是 DRAFT、PUBLISHED 或 ARCHIVED。")]
        public string? ArchiveStatus { get => _archiveStatus; set => _archiveStatus = NormalizeStatus(value); }
    }
}
