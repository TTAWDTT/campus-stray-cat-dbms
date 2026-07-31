using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models {
    public abstract class CatWriteRequest {
        private string? _catName;
        private string? _breed;
        private string? _colorPattern;
        private string? _personalityTags;
        private string? _mainAreaId;

        [StringLength(50, ErrorMessage = "猫咪名称不能超过 50 个字符。")]
        public string? CatName { get => _catName; set => _catName = NormalizeOptional(value); }

        [StringLength(50, ErrorMessage = "品种不能超过 50 个字符。")]
        public string? Breed { get => _breed; set => _breed = NormalizeOptional(value); }

        // 前端可提供预设花色（如橘色、狸花、黑白、三花、玳瑁）和“自定义”选项。
        // 选择“自定义”时显示文本输入框；后端接收并保存预设值或自定义内容的最终文本。
        [Required(ErrorMessage = "花色不能为空。")]
        [StringLength(100, ErrorMessage = "花色不能超过 100 个字符。")]
        public string? ColorPattern { get => _colorPattern; set => _colorPattern = NormalizeOptional(value); }

        // 前端建议显示为“猫咪性格评价”，使用多行文本框。
        // 占位提示：“例如：比较亲人，刚见面时有些胆小，熟悉后喜欢主动靠近，也比较爱叫。”
        // 辅助提示：“最多输入 200 字”。后端保存 Trim 后的完整文本，空白转为 null。
        [StringLength(200, ErrorMessage = "猫咪性格评价不能超过 200 个字符。")]
        public string? PersonalityTags { get => _personalityTags; set => _personalityTags = NormalizeOptional(value); }

        [StringLength(36, ErrorMessage = "主要区域 ID 不能超过 36 个字符。")]
        public string? MainAreaId { get => _mainAreaId; set => _mainAreaId = NormalizeOptional(value); }

        protected static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        protected static string? NormalizeStatus(string? value) => NormalizeOptional(value)?.ToUpperInvariant();
    }
}
