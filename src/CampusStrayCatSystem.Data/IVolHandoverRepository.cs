using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 志愿者交接记录数据访问接口，对应数据库表 VOL_HANDOVERS
    public interface IVolHandoverRepository
    {
        Task<IEnumerable<VolHandover>> GetAll();                                                       // 获取所有交接记录（按发起时间倒序）。
        Task<VolHandover?> GetById(string handoverId);                                                 // 按交接 ID 获取单条交接记录。
        Task<IEnumerable<VolHandover>> GetByFromVolunteer(string fromVolunteerId);                     // 查询某志愿者发起的所有交接（"我发起的交接"）。
        Task<IEnumerable<VolHandover>> GetByToVolunteer(string toVolunteerId);                         // 查询某志愿者需要确认的所有交接（"待我确认的交接"）。
        Task<IEnumerable<VolHandover>> GetByStatus(string status);                                     // 按状态筛选交接记录（用于"交接状态可查询"）。
        Task<IEnumerable<VolHandover>> GetByRelated(string relatedType, string relatedId);             // 按关联对象查询交接记录（如查询某投喂任务的所有交接历史）。
        Task<int> Create(VolHandover handover);                                                        // 提交交接
        Task<bool> Confirm(string handoverId, string fromVolunteerId, string toVolunteerId, string? relatedType, string? relatedId); // 原子确认交接
        Task<int> Reject(string handoverId);                                                           // 拒绝交接
        Task<int> Cancel(string handoverId);                                                           // 撤销交接
    }
}
