using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 医疗提醒仓储实现。
    /// 查询使用视图，新增和状态流转调用 Oracle Package。
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
                FROM V_MED_PENDING_REMINDERS
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
                FROM V_MED_REMINDERS
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
                FROM V_MED_REMINDERS
                WHERE REMINDERID = :ReminderID";

            return await QuerySingleAsync(sql, new { ReminderID = reminderId });
        }

        public async Task<int> CreateReminder(MedReminder reminder)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_RECORDID", reminder.RecordID, DbType.String);
            parameters.Add("P_CATID", reminder.CatID, DbType.String);
            parameters.Add("P_REMINDERTYPE", reminder.ReminderType, DbType.String);
            parameters.Add("P_RECEIVERUSERID", reminder.ReceiverUserID, DbType.String);
            parameters.Add("P_REMINDERTIME", reminder.ReminderTime, DbType.DateTime);
            parameters.Add("O_REMINDERID", dbType: DbType.String, direction: ParameterDirection.Output, size: 36);

            using var connection = CreateConnection();
            var rows = await connection.ExecuteAsync(
                "PKG_RESCUE_141516.CREATE_REMINDER",
                parameters,
                commandType: CommandType.StoredProcedure);

            reminder.ReminderID = parameters.Get<string>("O_REMINDERID");
            reminder.SendStatus = "PENDING";

            return rows;
        }

        public async Task<int> MarkSent(string reminderId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_REMINDERID", reminderId, DbType.String);

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "PKG_RESCUE_141516.MARK_REMINDER_SENT",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> Complete(string reminderId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("P_REMINDERID", reminderId, DbType.String);

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "PKG_RESCUE_141516.COMPLETE_REMINDER",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
