using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    /// <summary>
    /// 失踪预警的数据访问接口。
    /// 包括创建最后目击记录、创建预警，以及更新预警状态。
    /// </summary>
    public interface IMissingAlertRepository
    {
        Task<IEnumerable<CatMissingAlert>> GetAll();
        Task<IEnumerable<CatMissingAlert>> GetByCatId(string catId);
        Task<CatMissingAlert?> GetById(string alertId);
        Task<int> CreateSighting(CatSighting sighting);
        Task<int> CreateAlert(CatMissingAlert alert);
        Task<int> UpdateStatus(string alertId, string alertStatus, string? handlerUserId, string? remark);
    }
}