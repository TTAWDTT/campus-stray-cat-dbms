namespace CampusStrayCatSystem.Models
{
    /// <summary>
    /// 医疗提醒记录。
    /// 这类数据用于记录猫咪后续要做的疫苗、驱虫或绝育提醒。
    /// </summary>
    public class MedReminder
    {
        public string ReminderID { get; set; } = string.Empty;
        public string? RecordID { get; set; }
        public string? CatID { get; set; }
        public string ReminderType { get; set; } = string.Empty;
        public string? ReceiverUserID { get; set; }
        public DateTime? ReminderTime { get; set; }
        public string SendStatus { get; set; } = "PENDING";
    }
}