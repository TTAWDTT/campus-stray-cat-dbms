using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 投喂任务（志愿者排班）数据访问接口，对应数据库表 VOL_SHIFTS
    public interface IVolShiftRepository
    {
        Task<IEnumerable<VolShift>> GetAll();                           // 获取所有投喂任务（按计划开始时间倒序）
        Task<VolShift?> GetById(string shiftId);                        // 按任务 ID 获取单个投喂任务
        Task<IEnumerable<VolShift>> GetByVolunteer(string volunteerId); // 按志愿者 ID 查询其名下投喂任务（"我的投喂任务"）
        Task<IEnumerable<VolShift>> GetByPoint(string pointId);         // 按投喂点 ID 查询该点位的投喂任务
        Task<IEnumerable<VolShift>> GetByStatus(string status);         // 按状态筛选投喂任务（如查看所有"待执行"任务）
        Task<int> Create(VolShift shift);                               // 创建新的投喂任务
        Task<int> Update(VolShift shift);                               // 更新投喂任务基本信息
        Task<int> UpdateStatus(string shiftId, string status);          // 更新投喂任务状态
        Task<bool> Exists(string shiftId);                              // 判断投喂任务是否存在
    }
}
