using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    // 投喂打卡记录数据访问实现，对应数据库表 VOL_CHECKINS
    public class VolCheckInRepository : BaseRepository<VolCheckIn>, IVolCheckInRepository
    {
        public VolCheckInRepository(IConfiguration configuration) : base(configuration) { }

        // 查询全部投喂打卡记录，按签到时间倒序
        public async Task<IEnumerable<VolCheckIn>> GetAll()
        {
            const string sql = @"
                SELECT CHECKINID AS CheckInID,
                       SHIFTID AS ShiftID,
                       CHECKINTIME AS CheckInTime,
                       CHECKINSTATUS AS CheckInStatus
                FROM VOL_CHECKINS
                ORDER BY CHECKINTIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        // 按打卡 ID 获取单条投喂记录
        public async Task<VolCheckIn?> GetById(string checkInId)
        {
            const string sql = @"
                SELECT CHECKINID AS CheckInID,
                       SHIFTID AS ShiftID,
                       CHECKINTIME AS CheckInTime,
                       CHECKINSTATUS AS CheckInStatus
                FROM VOL_CHECKINS
                WHERE CHECKINID = :CheckInID";

            return await QuerySingleAsync(sql, new { CheckInID = checkInId });
        }

        // 查看某个投喂任务的完整投喂历史
        public async Task<IEnumerable<VolCheckIn>> GetByShift(string shiftId)
        {
            const string sql = @"
                SELECT CHECKINID AS CheckInID,
                       SHIFTID AS ShiftID,
                       CHECKINTIME AS CheckInTime,
                       CHECKINSTATUS AS CheckInStatus
                FROM VOL_CHECKINS
                WHERE SHIFTID = :ShiftID
                ORDER BY CHECKINTIME ASC";

            return await QueryAsync(sql, new { ShiftID = shiftId });
        }

        // 通过 JOIN VOL_SHIFTS 查询某志愿者的所有投喂打卡记录（"我的投喂历史"）
        public async Task<IEnumerable<VolCheckIn>> GetByVolunteer(string volunteerId)
        {
            const string sql = @"
                SELECT c.CHECKINID AS CheckInID,
                       c.SHIFTID AS ShiftID,
                       c.CHECKINTIME AS CheckInTime,
                       c.CHECKINSTATUS AS CheckInStatus
                FROM VOL_CHECKINS c
                INNER JOIN VOL_SHIFTS s ON s.SHIFTID = c.SHIFTID
                WHERE s.VOLUNTEERID = :VolunteerID
                ORDER BY c.CHECKINTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { VolunteerID = volunteerId });
        }

        // 记录投喂完成情况（事务）：1) 插入打卡记录；2) 把对应任务状态更新为 COMPLETED。任一步失败则回滚。
        public async Task CreateWithShiftCompleted(VolCheckIn checkIn)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 生成打卡记录主键
                checkIn.CheckInID = Guid.NewGuid().ToString();

                // 插入投喂打卡记录
                const string insertSql = @"
                    INSERT INTO VOL_CHECKINS (CHECKINID, SHIFTID, CHECKINTIME, CHECKINSTATUS)
                    VALUES (:CheckInID, :ShiftID, :CheckInTime, :CheckInStatus)";

                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    checkIn.CheckInID,
                    checkIn.ShiftID,
                    // 若未显式传入签到时间，则默认取当前时间
                    CheckInTime = checkIn.CheckInTime ?? DateTime.Now,
                    // 若未指定状态，默认为已签到
                    CheckInStatus = string.IsNullOrWhiteSpace(checkIn.CheckInStatus)
                        ? CheckInStatuses.CheckedIn
                        : checkIn.CheckInStatus
                });

                // 把对应投喂任务状态更新为“已完成”
                const string updateShiftSql = @"
                    UPDATE VOL_SHIFTS
                    SET SHIFTSTATUS = :ShiftStatus
                    WHERE SHIFTID = :ShiftID";

                await ExecuteAsync(connection, transaction, updateShiftSql, new
                {
                    ShiftStatus = ShiftStatuses.Completed,
                    checkIn.ShiftID
                });

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
