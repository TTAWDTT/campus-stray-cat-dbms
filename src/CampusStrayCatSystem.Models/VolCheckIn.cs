namespace CampusStrayCatSystem.Models
{
    // 投喂打卡状态集合 对应 VOL_CHECKINS.CHECKINSTATUS 属性
    public static class CheckInStatuses
    {
        public const string CheckedIn = "CHECKED_IN";    // 已签
        public const string Late =      "LATE";          // 迟到

        public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            CheckedIn, Late
        };

        // 判断状态字符串是否合法
        public static bool IsValid(string? status) => status != null && Allowed.Contains(status);
    }

    // 投喂打卡记录实体，对应数据库表 VOL_CHECKINS
    public class VolCheckIn
    {
        public string CheckInID { get; set; } = string.Empty;       // 签到记录ID（主键）
        public string ShiftID { get; set; } = string.Empty;         // 关联的投喂任务 ID（外键 VOL_SHIFTS.SHIFTID）
        public DateTime? CheckInTime { get; set; }                  // 实际签到时间
        public string? CheckInStatus { get; set; }                  // 打卡状态
    }
}