using CampusStrayCatSystem.Models;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 医疗提醒仓储实现。
    /// 这里直接操作 MED_REMINDERS 表。
    /// </summary>
    public class MedReminderRepository : BaseRepository<MedReminder>, IMedReminderRepository
    {
        public MedReminderRepository(IConfiguration configuration) : base(configuration)
        {
        }

        //异步任务
        public async Task<IEnumerable<MedReminder>> GetPendingReminders()
        {
            const string sql = @"
                SELECT REMINDERID AS ReminderID,
                       RECORDID AS RecordID,
                       CATID AS CatID,
                       REMINDERTYPE AS ReminderType,
                       RECEIVERUSERID AS ReceiverUserID,
                       REMINDERTIME AS ReminderTime,
                       SENDSTATUS AS SendStatus
                FROM MED_REMINDERS
                WHERE NVL(SENDSTATUS, 'PENDING') IN ('PENDING', 'SENT')
                ORDER BY REMINDERTIME";

            return await QueryAsync(sql);
        }

        public async Task<IEnumerable<MedReminder>> GetByCatId(string catId)
        {
            const string sql = @"
                SELECT REMINDERID AS ReminderID,
                       RECORDID AS RecordID,
                       CATID AS CatID,
                       REMINDERTYPE AS ReminderType,
                       RECEIVERUSERID AS ReceiverUserID,
                       REMINDERTIME AS ReminderTime,
                       SENDSTATUS AS SendStatus
                FROM MED_REMINDERS
                WHERE CATID = :CatID
                ORDER BY REMINDERTIME DESC";

            return await QueryAsync(sql, new { CatID = catId });
        }

        public async Task<MedReminder?> GetById(string reminderId)
        {
            const string sql = @"
                SELECT REMINDERID AS ReminderID,
                       RECORDID AS RecordID,
                       CATID AS CatID,
                       REMINDERTYPE AS ReminderType,
                       RECEIVERUSERID AS ReceiverUserID,
                       REMINDERTIME AS ReminderTime,
                       SENDSTATUS AS SendStatus
                FROM MED_REMINDERS
                WHERE REMINDERID = :ReminderID";

            return await QuerySingleAsync(sql, new { ReminderID = reminderId });
        }

        public async Task<int> CreateReminder(MedReminder reminder)
        {
            reminder.ReminderID = EnsureId(reminder.ReminderID);
            reminder.SendStatus = NormalizeStatus(reminder.SendStatus, "PENDING");

            const string sql = @"
                INSERT INTO MED_REMINDERS (
                    REMINDERID,
                    RECORDID,
                    CATID,
                    REMINDERTYPE,
                    RECEIVERUSERID,
                    REMINDERTIME,
                    SENDSTATUS
                ) VALUES (
                    :ReminderID,
                    :RecordID,
                    :CatID,
                    :ReminderType,
                    :ReceiverUserID,
                    :ReminderTime,
                    :SendStatus
                )";

            return await ExecuteAsync(sql, reminder);
        }

        public async Task<int> MarkSent(string reminderId)
        {
            const string sql = @"
                UPDATE MED_REMINDERS
                SET SENDSTATUS = 'SENT'
                WHERE REMINDERID = :ReminderID";

            return await ExecuteAsync(sql, new { ReminderID = reminderId });
        }

        public async Task<int> Complete(string reminderId)
        {
            const string sql = @"
                UPDATE MED_REMINDERS
                SET SENDSTATUS = 'COMPLETED'
                WHERE REMINDERID = :ReminderID";

            return await ExecuteAsync(sql, new { ReminderID = reminderId });
        }

        private static string EnsureId(string? id)
        {
            return string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        }

        private static string NormalizeStatus(string? status, string fallback)
        {
            return string.IsNullOrWhiteSpace(status) ? fallback : status.Trim().ToUpperInvariant();
        }
    }
}
