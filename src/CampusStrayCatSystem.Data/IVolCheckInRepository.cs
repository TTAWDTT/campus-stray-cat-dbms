using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 投喂打卡记录数据访问接口，对应数据库表 VOL_CHECKINS
    public interface IVolCheckInRepository
    {
        Task<IEnumerable<VolCheckIn>> GetAll();                           // 获取所有投喂打卡记录（按签到时间倒序）
        Task<VolCheckIn?> GetById(string checkInId);                      // 按打卡 ID 获取单条投喂记录
        Task<IEnumerable<VolCheckIn>> GetByShift(string shiftId);         // 按投喂任务 ID 查询该任务的投喂历史记录
        Task<IEnumerable<VolCheckIn>> GetByVolunteer(string volunteerId); // 按志愿者 ID 查询其所有投喂记录（通过 VOL_SHIFTS 关联）
        Task<bool> CreateWithShiftCompleted(VolCheckIn checkIn);          // 记录投喂并完成任务；任务已完成或状态不允许时返回 false
    }
}
