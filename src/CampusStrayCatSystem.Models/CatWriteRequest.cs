using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models {
    public abstract class CatWriteRequest {
        private string? _catName;
        private string? _breed;
        private string? _colorPattern;
        private string? _personalityTags;
        private string? _mainAreaId;

        [Utf8ByteLength(50, ErrorMessage = "猫咪名称内容不能超过数据库允许的 50 字节。")]
        public string? CatName { get => _catName; set => _catName = NormalizeOptional(value); }

        [Utf8ByteLength(50, ErrorMessage = "品种内容不能超过数据库允许的 50 字节。")]
        public string? Breed { get => _breed; set => _breed = NormalizeOptional(value); }

        // 前端可提供预设花色（如橘色、狸花、黑白、三花、玳瑁）和“自定义”选项。
        // 选择“自定义”时显示文本输入框；后端接收并保存预设值或自定义内容的最终文本。
        [Required(ErrorMessage = "花色不能为空。")]
        [Utf8ByteLength(100, ErrorMessage = "花色内容不能超过数据库允许的 100 字节。")]
        public string? ColorPattern { get => _colorPattern; set => _colorPattern = NormalizeOptional(value); }

        // 前端可提供预设性格特征（如亲人、胆小、爱叫）和自定义标签。
        // 多个标签使用英文逗号分隔；后端会去除空标签和多余空格，并将中文逗号统一为英文逗号。
        [Utf8ByteLength(200, ErrorMessage = "性格标签内容不能超过数据库允许的 200 字节。")]
        public string? PersonalityTags { get => _personalityTags; set => _personalityTags = NormalizeTags(value); }

        [Utf8ByteLength(36, ErrorMessage = "主要区域 ID 不能超过数据库允许的 36 字节。")]
        public string? MainAreaId { get => _mainAreaId; set => _mainAreaId = NormalizeOptional(value); }

        protected static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        protected static string? NormalizeStatus(string? value) => NormalizeOptional(value)?.ToUpperInvariant();

        private static string? NormalizeTags(string? value) {
            var normalized = NormalizeOptional(value)?.Replace('，', ',');
            if (normalized == null) {
                return null;}

            var tags = normalized.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return tags.Length == 0 ? null : string.Join(',', tags);}
    }
}
