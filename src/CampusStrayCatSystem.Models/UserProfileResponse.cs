namespace CampusStrayCatSystem.Models
{
    public class UserProfileResponse
    {
        public string UserID { get; set; } = string.Empty;
        public string RoleID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? RealName { get; set; }
        public string? StudentNo { get; set; }
        public string? Phone { get; set; }
        public string? VerifyStatus { get; set; }
        public string? Status { get; set; }
        public string? RoleName { get; set; }
        public string? PermissionScope { get; set; }
    }
}
