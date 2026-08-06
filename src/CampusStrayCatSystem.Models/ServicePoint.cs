using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class ServicePoint
    {
        [StringLength(36, ErrorMessage = "点位 ID 不能超过 36 个字符。")]
        public string PointID { get; set; } = string.Empty;

        [StringLength(36, ErrorMessage = "区域 ID 不能超过 36 个字符。")]
        public string? AreaID { get; set; }

        [StringLength(100, ErrorMessage = "点位名称不能超过 100 个字符。")]
        public string PointName { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "点位类型不能超过 30 个字符。")]
        public string? PointType { get; set; }

        [Range(-180, 180, ErrorMessage = "经度必须在 -180 到 180 之间。")]
        public decimal? Longitude { get; set; }

        [Range(-90, 90, ErrorMessage = "纬度必须在 -90 到 90 之间。")]
        public decimal? Latitude { get; set; }

        [StringLength(20, ErrorMessage = "设施状态不能超过 20 个字符。")]
        public string? FacilityStatus { get; set; }

        public DateTime? DeployTime { get; set; }
    }
}
