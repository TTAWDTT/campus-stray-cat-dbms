namespace CampusStrayCatSystem.Models
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PermissionScope { get; set; } = string.Empty;
    }
}
