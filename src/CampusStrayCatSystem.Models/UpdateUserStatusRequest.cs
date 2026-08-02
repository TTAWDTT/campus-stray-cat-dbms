using System.ComponentModel.DataAnnotations;

namespace CampusStrayCatSystem.Models
{
    public class UpdateUserStatusRequest
    {
        [Required(ErrorMessage = "Status 不能为空。")]
        [Utf8ByteLength(20, ErrorMessage = "Status 不能超过数据库允许的 20 字节。")]
        public string Status { get; set; } = string.Empty;
    }
}
