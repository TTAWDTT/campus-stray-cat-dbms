using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "RoleID 不能为空。")]
        [Utf8ByteLength(36, ErrorMessage = "RoleID 不能超过数据库允许的 36 字节。")]
        public string RoleID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username 不能为空。")]
        [Utf8ByteLength(50, ErrorMessage = "Username 不能超过数据库允许的 50 字节。")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password 不能为空。")]
        [MinLength(6, ErrorMessage = "Password 至少 6 位。")]
        [Utf8ByteLength(255, ErrorMessage = "Password 不能超过数据库允许的 255 字节。")]
        public string Password { get; set; } = string.Empty;

        [Utf8ByteLength(50, ErrorMessage = "RealName 不能超过数据库允许的 50 字节。")]
        public string? RealName { get; set; }

        [Utf8ByteLength(30, ErrorMessage = "StudentNo 不能超过数据库允许的 30 字节。")]
        public string? StudentNo { get; set; }

        [Utf8ByteLength(20, ErrorMessage = "Phone 不能超过数据库允许的 20 字节。")]
        public string? Phone { get; set; }

        [Utf8ByteLength(20, ErrorMessage = "VerifyStatus 不能超过数据库允许的 20 字节。")]
        public string? VerifyStatus { get; set; }

        /// <summary>
        /// 可选。仅接受 ACTIVE / DISABLED；缺省为 ACTIVE。客户端不可指定 UserID / PasswordHash。
        /// </summary>
        [Utf8ByteLength(20, ErrorMessage = "Status 不能超过数据库允许的 20 字节。")]
        public string? Status { get; set; }
    }
}
