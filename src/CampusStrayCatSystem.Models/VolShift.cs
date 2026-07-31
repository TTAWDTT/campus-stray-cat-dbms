namespace CampusStrayCatSystem.Models
{
    // 投喂任务状态集合 对应 VOL_SHIFTS.SHIFTSTATUS 属性
    public static class ShiftStatuses
    {
        public const string Planned =    "PLANNED";         // 已排班，尚未指派
        public const string Assigned =   "ASSIGNED";        // 已分配，待志愿者执行
        public const string InProgress = "IN_PROGRESS";     // 执行中
        public const string Completed =  "COMPLETED";       // 已完成
        public const string Missed =     "MISSED";          // 逾期未完成

        public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            Planned, Assigned, InProgress, Completed, Missed
        };

        // 判断状态字符串是否合法
        public static bool IsValid(string? status) => status != null && Allowed.Contains(status);
    }
    // 投喂任务（志愿者排班）实体，对应数据库表 VOL_SHIFTS
    public class VolShift
    {
        public string ShiftID { get; set; } = string.Empty;     // 任务ID（主键）
        public string VolunteerID { get; set; } = string.Empty; // 负责的志愿者 ID（外键 VOL_VOLUNTEERS.VOLUNTEERID）
        public string? PointID { get; set; }                    // 投喂地点 ID（外键 MAP_SERVICEPOINTS.POINTID）
        public DateTime? PlanStartTime { get; set; }            // 计划开始时间
        public DateTime? PlanEndTime { get; set; }              // 计划结束时间
        public string? ShiftStatus { get; set; }                // 任务状态
    }
}