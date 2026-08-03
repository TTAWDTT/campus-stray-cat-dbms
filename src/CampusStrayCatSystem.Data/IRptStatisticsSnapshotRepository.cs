using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 统计快照数据访问接口，对应数据库表 RPT_STATISTICSSNAPSHOTS
    public interface IRptStatisticsSnapshotRepository
    {
        Task<IEnumerable<RptStatisticsSnapshot>> GetAll();                                     // 获取所有统计快照
        Task<RptStatisticsSnapshot?> GetById(string snapshotId);                               // 按快照 ID 获取单条统计快照
        Task<IEnumerable<RptStatisticsSnapshot>> GetByMetric(string metricCode);               // 按指标代码查询统计快照（如查所有 TOTAL_DONATION 指标）
        Task<IEnumerable<RptStatisticsSnapshot>> GetByDimension(string dimensionType, string dimensionValue); // 按维度查询统计快照（如按项目维度 PROJECT 查询）
        Task GenerateProjectReportSnapshot(string projectId, decimal totalDonation, decimal totalExpense, decimal netBalance, int donationCount); // 为指定众筹项目生成统计报表快照（事务）：一次性写入该项目的关键指标（总捐赠、已通过支出、净余额、捐赠笔数）到快照表
    }
}
