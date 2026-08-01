namespace CampusStrayCatSystem.Models
{
    public class LoginResponse
    {
        public string UserID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? RealName { get; set; }
        public string RoleID { get; set; } = string.Empty;
        public string? RoleName { get; set; }
        public string? PermissionScope { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
