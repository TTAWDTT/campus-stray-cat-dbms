using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 紧急救助上报的数据访问接口。
    /// 负责提交上报、分配处理人，以及更新处理结果。
    /// </summary>
    public interface IEmergencyReportRepository
    {
        Task<IEnumerable<EmergencyReport>> GetAll();
        Task<EmergencyReport?> GetById(string reportId);
        Task<int> Create(EmergencyReport report);
        Task<int> AssignHandler(string reportId, string? handlerUserId);
        Task<int> UpdateStatus(string reportId, string status, string? processResult);
    }
}