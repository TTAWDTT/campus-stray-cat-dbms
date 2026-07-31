using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    // 统计快照数据访问实现，对应数据库表 RPT_STATISTICSSNAPSHOTS
    // GenerateProjectReportSnapshot 把项目的关键财务指标一次性写入快照表，供报表查询使用
    public class RptStatisticsSnapshotRepository : BaseRepository<RptStatisticsSnapshot>, IRptStatisticsSnapshotRepository
    {
        public RptStatisticsSnapshotRepository(IConfiguration configuration) : base(configuration) { }

        // 获取所有统计快照，按生成时间倒序
        public async Task<IEnumerable<RptStatisticsSnapshot>> GetAll()
        {
            const string sql = @"
                SELECT SNAPSHOTID AS SnapshotID,
                       SNAPSHOTDATE AS SnapshotDate,
                       METRICCODE AS MetricCode,
                       DIMENSIONTYPE AS DimensionType,
                       DIMENSIONVALUE AS DimensionValue,
                       METRICVALUE AS MetricValue,
                       UNIT AS Unit,
                       GENERATETIME AS GenerateTime,
                       REMARK AS Remark
                FROM RPT_STATISTICSSNAPSHOTS
                ORDER BY GENERATETIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        // 按快照 ID 获取单条统计快照
        public async Task<RptStatisticsSnapshot?> GetById(string snapshotId)
        {
            const string sql = @"
                SELECT SNAPSHOTID AS SnapshotID,
                       SNAPSHOTDATE AS SnapshotDate,
                       METRICCODE AS MetricCode,
                       DIMENSIONTYPE AS DimensionType,
                       DIMENSIONVALUE AS DimensionValue,
                       METRICVALUE AS MetricValue,
                       UNIT AS Unit,
                       GENERATETIME AS GenerateTime,
                       REMARK AS Remark
                FROM RPT_STATISTICSSNAPSHOTS
                WHERE SNAPSHOTID = :SnapshotID";

            return await QuerySingleAsync(sql, new { SnapshotID = snapshotId });
        }

        // 按指标代码查询统计快照
        public async Task<IEnumerable<RptStatisticsSnapshot>> GetByMetric(string metricCode)
        {
            const string sql = @"
                SELECT SNAPSHOTID AS SnapshotID,
                       SNAPSHOTDATE AS SnapshotDate,
                       METRICCODE AS MetricCode,
                       DIMENSIONTYPE AS DimensionType,
                       DIMENSIONVALUE AS DimensionValue,
                       METRICVALUE AS MetricValue,
                       UNIT AS Unit,
                       GENERATETIME AS GenerateTime,
                       REMARK AS Remark
                FROM RPT_STATISTICSSNAPSHOTS
                WHERE METRICCODE = :MetricCode
                ORDER BY GENERATETIME DESC NULLS LAST";

            return await QueryAsync(sql, new { MetricCode = metricCode });
        }

        // 按维度查询统计快照
        public async Task<IEnumerable<RptStatisticsSnapshot>> GetByDimension(string dimensionType, string dimensionValue)
        {
            const string sql = @"
                SELECT SNAPSHOTID AS SnapshotID,
                       SNAPSHOTDATE AS SnapshotDate,
                       METRICCODE AS MetricCode,
                       DIMENSIONTYPE AS DimensionType,
                       DIMENSIONVALUE AS DimensionValue,
                       METRICVALUE AS MetricValue,
                       UNIT AS Unit,
                       GENERATETIME AS GenerateTime,
                       REMARK AS Remark
                FROM RPT_STATISTICSSNAPSHOTS
                WHERE DIMENSIONTYPE = :DimensionType
                  AND DIMENSIONVALUE = :DimensionValue
                ORDER BY GENERATETIME DESC NULLS LAST";

            return await QueryAsync(sql, new
            {
                DimensionType = dimensionType,
                DimensionValue = dimensionValue
            });
        }

        // 为指定项目生成统计报表快照（事务）：写入 4 条指标记录（总捐赠、已通过支出、净余额、捐赠笔数），以 PROJECT 维度、项目 ID 为维度值
        public async Task GenerateProjectReportSnapshot(
            string projectId,
            decimal totalDonation,
            decimal totalExpense,
            decimal netBalance,
            int donationCount)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var snapshotDate = DateTime.Today;
                var generateTime = DateTime.Now;

                const string insertSql = @"
                    INSERT INTO RPT_STATISTICSSNAPSHOTS (SNAPSHOTID, SNAPSHOTDATE, METRICCODE, DIMENSIONTYPE,
                                                         DIMENSIONVALUE, METRICVALUE, UNIT, GENERATETIME, REMARK)
                    VALUES (:SnapshotID, :SnapshotDate, :MetricCode, :DimensionType,
                            :DimensionValue, :MetricValue, :Unit, :GenerateTime, :Remark)";

                // 指标1：总捐赠额
                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    SnapshotID = Guid.NewGuid().ToString(),
                    SnapshotDate = snapshotDate,
                    MetricCode = "TOTAL_DONATION",
                    DimensionType = "PROJECT",
                    DimensionValue = projectId,
                    MetricValue = totalDonation,
                    Unit = "CNY",
                    GenerateTime = generateTime,
                    Remark = "项目累计捐赠总额"
                });

                // 指标2：已审核通过的支出总额
                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    SnapshotID = Guid.NewGuid().ToString(),
                    SnapshotDate = snapshotDate,
                    MetricCode = "TOTAL_EXPENSE",
                    DimensionType = "PROJECT",
                    DimensionValue = projectId,
                    MetricValue = totalExpense,
                    Unit = "CNY",
                    GenerateTime = generateTime,
                    Remark = "项目已审核通过支出总额"
                });

                // 指标3：净余额 = 总捐赠 - 已通过支出
                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    SnapshotID = Guid.NewGuid().ToString(),
                    SnapshotDate = snapshotDate,
                    MetricCode = "NET_BALANCE",
                    DimensionType = "PROJECT",
                    DimensionValue = projectId,
                    MetricValue = netBalance,
                    Unit = "CNY",
                    GenerateTime = generateTime,
                    Remark = "项目净余额（捐赠-支出）"
                });

                // 指标4：捐赠笔数
                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    SnapshotID = Guid.NewGuid().ToString(),
                    SnapshotDate = snapshotDate,
                    MetricCode = "DONATION_COUNT",
                    DimensionType = "PROJECT",
                    DimensionValue = projectId,
                    MetricValue = donationCount,
                    Unit = "COUNT",
                    GenerateTime = generateTime,
                    Remark = "项目捐赠笔数"
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
