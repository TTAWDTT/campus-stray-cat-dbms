using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models.DTOs
{
    /// <summary>
    /// 加入黑名单请求DTO
    /// </summary>
    public class AddBlacklistDto
    {
        [Required(ErrorMessage = "用户ID不能为空")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "拉黑原因类型不能为空")]
        [MaxLength(50, ErrorMessage = "原因类型长度不能超过50")]
        public string ReasonType { get; set; }

        [Required(ErrorMessage = "详细原因不能为空")]
        [MaxLength(500, ErrorMessage = "详细原因长度不能超过500")]
        public string ReasonDetail { get; set; }

        public string ApplicationId { get; set; }  // 可选
    }
}