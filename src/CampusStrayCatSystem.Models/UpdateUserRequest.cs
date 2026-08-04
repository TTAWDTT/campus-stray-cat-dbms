using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "RoleID 不能为空。")]
        [Utf8ByteLength(36, ErrorMessage = "RoleID 不能超过数据库允许的 36 字节。")]
        public string RoleID { get; set; } = string.Empty;

        [Utf8ByteLength(50, ErrorMessage = "RealName 不能超过数据库允许的 50 字节。")]
        public string? RealName { get; set; }

        [Utf8ByteLength(30, ErrorMessage = "StudentNo 不能超过数据库允许的 30 字节。")]
        public string? StudentNo { get; set; }

        [Utf8ByteLength(20, ErrorMessage = "Phone 不能超过数据库允许的 20 字节。")]
        public string? Phone { get; set; }

        [Utf8ByteLength(20, ErrorMessage = "VerifyStatus 不能超过数据库允许的 20 字节。")]
        public string? VerifyStatus { get; set; }

        /// <summary>
        /// 可选。仅接受 ACTIVE / DISABLED。Username / PasswordHash / UserID 由服务端保护，不接受客户端改写。
        /// </summary>
        [Utf8ByteLength(20, ErrorMessage = "Status 不能超过数据库允许的 20 字节。")]
        public string? Status { get; set; }
    }
}
