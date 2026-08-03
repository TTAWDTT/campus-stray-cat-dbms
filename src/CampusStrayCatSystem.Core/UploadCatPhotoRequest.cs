using System.ComponentModel.DataAnnotations;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Core {
    public class UploadCatPhotoRequest {
        private string? _uploadUserID;

        [Required(ErrorMessage = "照片文件不能为空。")]
        public IFormFile? File { get; set; }

        [Required(ErrorMessage = "上传用户 ID 不能为空。")]
        [Utf8ByteLength(36, ErrorMessage = "上传用户 ID 不能超过数据库允许的 36 字节。")]
        public string? UploadUserID {
            get => _uploadUserID;
            set => _uploadUserID = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        [Range(0, 1, ErrorMessage = "主图标志只能是 0 或 1。")]
        public int IsPrimary { get; set; }
    }
}
