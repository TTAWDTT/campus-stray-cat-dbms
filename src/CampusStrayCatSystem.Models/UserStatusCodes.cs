namespace CampusStrayCatSystem.Models
{
    /// <summary>
    /// 用户模块状态契约（与 SYS_USERS / API 文档一致，仅英文枚举）。
    /// </summary>
    public static class UserStatusCodes
    {
        public const string Active = "ACTIVE";
        public const string Disabled = "DISABLED";

        public static bool IsKnown(string? status) =>
            status == Active || status == Disabled;

        public static bool IsActive(string? status) => status == Active;
    }

    public static class UserVerifyStatusCodes
    {
        public const string Verified = "VERIFIED";
        public const string Unverified = "UNVERIFIED";

        public static bool IsKnown(string? status) =>
            string.IsNullOrWhiteSpace(status) ||
            status == Verified ||
            status == Unverified;
    }
}
