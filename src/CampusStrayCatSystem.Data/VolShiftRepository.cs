using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 投喂任务（志愿者排班）数据访问实现，对应数据库表 VOL_SHIFTS
    public class VolShiftRepository : BaseRepository<VolShift>, IVolShiftRepository
    {
        public VolShiftRepository(IConfiguration configuration) : base(configuration) { }

        // 查询全部投喂任务，按计划开始时间倒序，空值排最后
        public async Task<IEnumerable<VolShift>> GetAll()
        {
            const string sql = @"
                SELECT SHIFTID AS ShiftID,
                       VOLUNTEERID AS VolunteerID,
                       POINTID AS PointID,
                       PLANSTARTTIME AS PlanStartTime,
                       PLANENDTIME AS PlanEndTime,
                       SHIFTSTATUS AS ShiftStatus
                FROM VOL_SHIFTS
                ORDER BY PLANSTARTTIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        // 按任务 ID 查询单个投喂任务
        public async Task<VolShift?> GetById(string shiftId)
        {
            const string sql = @"
                SELECT SHIFTID AS ShiftID,
                       VOLUNTEERID AS VolunteerID,
                       POINTID AS PointID,
                       PLANSTARTTIME AS PlanStartTime,
                       PLANENDTIME AS PlanEndTime,
                       SHIFTSTATUS AS ShiftStatus
                FROM VOL_SHIFTS
                WHERE SHIFTID = :ShiftID";

            return await QuerySingleAsync(sql, new { ShiftID = shiftId });
        }

        // 按志愿者 ID 查询投喂任务
        public async Task<IEnumerable<VolShift>> GetByVolunteer(string volunteerId)
        {
            const string sql = @"
                SELECT SHIFTID AS ShiftID,
                       VOLUNTEERID AS VolunteerID,
                       POINTID AS PointID,
                       PLANSTARTTIME AS PlanStartTime,
                       PLANENDTIME AS PlanEndTime,
                       SHIFTSTATUS AS ShiftStatus
                FROM VOL_SHIFTS
                WHERE VOLUNTEERID = :VolunteerID
                ORDER BY PLANSTARTTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { VolunteerID = volunteerId });
        }

        // 按投喂点 ID 查询投喂任务
        public async Task<IEnumerable<VolShift>> GetByPoint(string pointId)
        {
            const string sql = @"
                SELECT SHIFTID AS ShiftID,
                       VOLUNTEERID AS VolunteerID,
                       POINTID AS PointID,
                       PLANSTARTTIME AS PlanStartTime,
                       PLANENDTIME AS PlanEndTime,
                       SHIFTSTATUS AS ShiftStatus
                FROM VOL_SHIFTS
                WHERE POINTID = :PointID
                ORDER BY PLANSTARTTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { PointID = pointId });
        }

        // 按状态筛选投喂任务
        public async Task<IEnumerable<VolShift>> GetByStatus(string status)
        {
            const string sql = @"
                SELECT SHIFTID AS ShiftID,
                       VOLUNTEERID AS VolunteerID,
                       POINTID AS PointID,
                       PLANSTARTTIME AS PlanStartTime,
                       PLANENDTIME AS PlanEndTime,
                       SHIFTSTATUS AS ShiftStatus
                FROM VOL_SHIFTS
                WHERE SHIFTSTATUS = :ShiftStatus
                ORDER BY PLANSTARTTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { ShiftStatus = status });
        }

        // 创建新的投喂任务
        public async Task<int> Create(VolShift shift)
        {
            shift.ShiftID = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO VOL_SHIFTS (SHIFTID, VOLUNTEERID, POINTID,
                                        PLANSTARTTIME, PLANENDTIME, SHIFTSTATUS)
                VALUES (:ShiftID, :VolunteerID, :PointID,
                        :PlanStartTime, :PlanEndTime, :ShiftStatus)";

            return await ExecuteAsync(sql, new
            {
                shift.ShiftID,
                shift.VolunteerID,
                shift.PointID,
                shift.PlanStartTime,
                shift.PlanEndTime,
                shift.ShiftStatus
            });
        }

        // 更新投喂任务基本信息
        public async Task<int> Update(VolShift shift)
        {
            const string sql = @"
                UPDATE VOL_SHIFTS
                SET VOLUNTEERID = :VolunteerID,
                    POINTID = :PointID,
                    PLANSTARTTIME = :PlanStartTime,
                    PLANENDTIME = :PlanEndTime,
                    SHIFTSTATUS = :ShiftStatus
                WHERE SHIFTID = :ShiftID";

            return await ExecuteAsync(sql, new
            {
                shift.VolunteerID,
                shift.PointID,
                shift.PlanStartTime,
                shift.PlanEndTime,
                shift.ShiftStatus,
                shift.ShiftID
            });
        }

        // 更新投喂任务状态
        public async Task<int> UpdateStatus(string shiftId, string status)
        {
            const string sql = @"
                UPDATE VOL_SHIFTS
                SET SHIFTSTATUS = :ShiftStatus
                WHERE SHIFTID = :ShiftID";

            return await ExecuteAsync(sql, new
            {
                ShiftStatus = status,
                ShiftID = shiftId
            });
        }

        // 判断投喂任务是否存在
        public async Task<bool> Exists(string shiftId)
        {
            const string sql = "SELECT COUNT(1) FROM VOL_SHIFTS WHERE SHIFTID = :ShiftID";
            var count = await QuerySingleAsync<int>(sql, new { ShiftID = shiftId });
            return count > 0;
        }
    }
}
