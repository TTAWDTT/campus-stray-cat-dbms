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
                       LONGITUDE AS Longitude,
                       LATITUDE AS Latitude,
                       PHOTOURL AS PhotoUrl,
                       DISTANCEMETERS AS DistanceMeters,
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
                       LONGITUDE AS Longitude,
                       LATITUDE AS Latitude,
                       PHOTOURL AS PhotoUrl,
                       DISTANCEMETERS AS DistanceMeters,
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
                       LONGITUDE AS Longitude,
                       LATITUDE AS Latitude,
                       PHOTOURL AS PhotoUrl,
                       DISTANCEMETERS AS DistanceMeters,
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
                       c.LONGITUDE AS Longitude,
                       c.LATITUDE AS Latitude,
                       c.PHOTOURL AS PhotoUrl,
                       c.DISTANCEMETERS AS DistanceMeters,
                       c.CHECKINSTATUS AS CheckInStatus
                FROM VOL_CHECKINS c
                INNER JOIN VOL_SHIFTS s ON s.SHIFTID = c.SHIFTID
                WHERE s.VOLUNTEERID = :VolunteerID
                ORDER BY c.CHECKINTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { VolunteerID = volunteerId });
        }

        // 记录投喂完成情况；仅未完成且没有打卡记录的任务可以执行一次。
        public async Task<bool> CreateWithShiftCompleted(VolCheckIn checkIn)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 生成打卡记录主键
                checkIn.CheckInID = Guid.NewGuid().ToString();
                checkIn.CheckInTime ??= DateTime.Now;
                checkIn.CheckInStatus = string.IsNullOrWhiteSpace(checkIn.CheckInStatus)
                    ? CheckInStatuses.CheckedIn
                    : checkIn.CheckInStatus.ToUpperInvariant();

                // 条件更新同时锁定任务，防止重复或并发打卡。
                const string updateShiftSql = @"
                    UPDATE VOL_SHIFTS
                    SET SHIFTSTATUS = :ShiftStatus
                    WHERE SHIFTID = :ShiftID
                      AND UPPER(NVL(SHIFTSTATUS, 'PLANNED')) IN ('PLANNED', 'ASSIGNED', 'IN_PROGRESS')
                      AND NOT EXISTS (
                          SELECT 1 FROM VOL_CHECKINS WHERE SHIFTID = :ShiftID
                      )";

                var updatedShifts = await ExecuteAsync(connection, transaction, updateShiftSql, new
                {
                    ShiftStatus = ShiftStatuses.Completed,
                    checkIn.ShiftID
                });

                if (updatedShifts != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                // 插入投喂打卡记录
                const string insertSql = @"
                    INSERT INTO VOL_CHECKINS (CHECKINID, SHIFTID, CHECKINTIME, LONGITUDE,
                                              LATITUDE, PHOTOURL, DISTANCEMETERS, CHECKINSTATUS)
                    VALUES (:CheckInID, :ShiftID, :CheckInTime, :Longitude,
                            :Latitude, :PhotoUrl, :DistanceMeters, :CheckInStatus)";

                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    checkIn.CheckInID,
                    checkIn.ShiftID,
                    checkIn.CheckInTime,
                    checkIn.Longitude,
                    checkIn.Latitude,
                    checkIn.PhotoUrl,
                    checkIn.DistanceMeters,
                    checkIn.CheckInStatus
                });

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
