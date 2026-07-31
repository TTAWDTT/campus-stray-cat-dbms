using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 医疗提醒的数据访问接口。
    /// 负责创建提醒、查询待处理提醒，以及更新提醒状态。
    /// </summary>
    public interface IMedReminderRepository
    {
        Task<IEnumerable<MedReminder>> GetPendingReminders();
        Task<IEnumerable<MedReminder>> GetByCatId(string catId);
        Task<MedReminder?> GetById(string reminderId);
        Task<int> CreateReminder(MedReminder reminder);
        Task<int> MarkSent(string reminderId);
        Task<int> Complete(string reminderId);
    }
}