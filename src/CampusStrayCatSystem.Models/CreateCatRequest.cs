using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models {
    public class CreateCatRequest : CatWriteRequest {
        private string? _gender = CatStatusCodes.GenderUnknown;
        private int? _sterilizedFlag = 0;
        private int? _earTipFlag = 0;
        private string? _lifeStatus = CatStatusCodes.LifeOnCampus;

        [StringLength(10, ErrorMessage = "性别代码不能超过 10 个字符。")]
        [RegularExpression(CatStatusCodes.GenderPattern, ErrorMessage = "性别只能是 UNKNOWN、MALE 或 FEMALE。")]
        public string? Gender { get => _gender; set => _gender = NormalizeStatus(value) ?? CatStatusCodes.GenderUnknown; }

        [Range(0, 1, ErrorMessage = "绝育标志只能是 0 或 1。")]
        public int? SterilizedFlag { get => _sterilizedFlag; set => _sterilizedFlag = value ?? 0; }

        [Range(0, 1, ErrorMessage = "剪耳标志只能是 0 或 1。")]
        public int? EarTipFlag { get => _earTipFlag; set => _earTipFlag = value ?? 0; }

        [StringLength(20, ErrorMessage = "生活状态代码不能超过 20 个字符。")]
        [RegularExpression(CatStatusCodes.LifeStatusPattern, ErrorMessage = "生活状态只能是 ON_CAMPUS、MISSING、ADOPTED 或 DECEASED。")]
        public string? LifeStatus { get => _lifeStatus; set => _lifeStatus = NormalizeStatus(value) ?? CatStatusCodes.LifeOnCampus; }
    }
}
