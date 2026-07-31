namespace CampusStrayCatSystem.Models
{
    // 交接状态集合 对应 VOL_HANDOVERS.HANDOVERSTATUS 属性
    public static class HandoverStatuses
    {
        public const string Pending =    "PENDING";       // 待确认：发起方已提交，等待接收方确认
        public const string Confirmed =  "CONFIRMED";     // 已确认：接收方已接受交接
        public const string Rejected =   "REJECTED";      // 已拒绝：接收方拒绝交接
        public const string Cancelled =  "CANCELLED";     // 已撤销：发起方撤销交接

        public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            Pending, Confirmed, Rejected, Cancelled
        };

        // 判断状态是否合法
        public static bool IsValid(string? status) => status != null && Allowed.Contains(status);
    }

    // 志愿者交接记录实体，对应数据库表 VOL_HANDOVERS
    // 交接对象通过 RelatedType + RelatedID 多态引用，目前仅用于投喂任务（RelatedType='SHIFT'）
    public class VolHandover
    {
        public string HandoverID { get; set; } = string.Empty;            // 交接记录ID（主键）
        public string FromVolunteerID { get; set; } = string.Empty;       // 发起方志愿者 ID（外键 VOL_VOLUNTEERS.VOLUNTEERID）
        public string ToVolunteerID { get; set; } = string.Empty;         // 接收方志愿者 ID（外键 VOL_VOLUNTEERS.VOLUNTEERID）
        public string? RelatedType { get; set; }                          // 关联对象类型（多态），目前固定为 "SHIFT" 关联投喂任务
        public string? RelatedID { get; set; }                            // 关联对象 ID（多态外键），如对应的 ShiftID
        public DateTime? ApplyTime { get; set; }                          // 交接发起时间
        public DateTime? ConfirmTime { get; set; }                        // 交接确认时间
        public string? HandoverStatus { get; set; }                       // 交接状态
        public string? Remark { get; set; }                               // 备注说明
    }
}