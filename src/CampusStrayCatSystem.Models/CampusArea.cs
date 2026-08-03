using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class CampusArea
    {
        [StringLength(36, ErrorMessage = "区域 ID 不能超过 36 个字符。")]
        public string AreaID { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "区域名称不能超过 100 个字符。")]
        public string AreaName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "校区名称不能超过 100 个字符。")]
        public string? CampusName { get; set; }

        [StringLength(36, ErrorMessage = "父区域 ID 不能超过 36 个字符。")]
        public string? ParentAreaID { get; set; }

        [StringLength(30, ErrorMessage = "区域类型不能超过 30 个字符。")]
        public string? AreaType { get; set; }

        [StringLength(20, ErrorMessage = "风险等级不能超过 20 个字符。")]
        public string? RiskLevel { get; set; }

        public string? GeoBoundary { get; set; }
    }
}
