using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    // 统计报表控制器，对应数据库表 RPT_STATISTICSSNAPSHOTS
    // 提供统计快照的生成与查询
    [Route("api/statistics-reports")]
    [ApiController]
    [Authorize]
    public class StatisticsReportsController : ControllerBase
    {
        private readonly IRptStatisticsSnapshotRepository _snapshotRepository;
        private readonly IFundCrowdfundingProjectRepository _projectRepository;
        private readonly IFundDonationRepository _donationRepository;
        private readonly IFundExpenseRecordRepository _expenseRecordRepository;

        public StatisticsReportsController(
            IRptStatisticsSnapshotRepository snapshotRepository,
            IFundCrowdfundingProjectRepository projectRepository,
            IFundDonationRepository donationRepository,
            IFundExpenseRecordRepository expenseRecordRepository)
        {
            _snapshotRepository = snapshotRepository;
            _projectRepository = projectRepository;
            _donationRepository = donationRepository;
            _expenseRecordRepository = expenseRecordRepository;
        }

        // 获取所有统计快照
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RptStatisticsSnapshot>>> GetAll()
        {
            var snapshots = await _snapshotRepository.GetAll();
            return Ok(snapshots ?? new List<RptStatisticsSnapshot>());
        }

        // 按快照 ID 获取单条统计快照
        [HttpGet("snapshot/{id}")]
        public async Task<ActionResult<RptStatisticsSnapshot>> GetById(string id)
        {
            var snapshot = await _snapshotRepository.GetById(id);
            if (snapshot == null)
                return NotFound($"未找到 ID 为 {id} 的统计快照。");

            return Ok(snapshot);
        }

        // 按指标代码查询统计快照（如 TOTAL_DONATION、TOTAL_EXPENSE、NET_BALANCE）
        [HttpGet("by-metric/{metricCode}")]
        public async Task<ActionResult<IEnumerable<RptStatisticsSnapshot>>> GetByMetric(string metricCode)
        {
            if (string.IsNullOrWhiteSpace(metricCode) || !StatisticCodes.MetricCodes.Contains(metricCode.Trim()))
                return BadRequest($"指标代码必须是 {string.Join("、", StatisticCodes.MetricCodes)}。");

            var snapshots = await _snapshotRepository.GetByMetric(metricCode.Trim().ToUpperInvariant());
            return Ok(snapshots ?? new List<RptStatisticsSnapshot>());
        }

        // 按维度查询统计快照（如按项目维度 PROJECT/{projectId}）
        [HttpGet("by-dimension/{dimensionType}/{dimensionValue}")]
        public async Task<ActionResult<IEnumerable<RptStatisticsSnapshot>>> GetByDimension(string dimensionType, string dimensionValue)
        {
            if (string.IsNullOrWhiteSpace(dimensionType) || !StatisticCodes.DimensionTypes.Contains(dimensionType.Trim()))
                return BadRequest($"维度类型必须是 {string.Join("、", StatisticCodes.DimensionTypes)}。");
            if (string.IsNullOrWhiteSpace(dimensionValue))
                return BadRequest("维度值不能为空。");

            var snapshots = await _snapshotRepository.GetByDimension(dimensionType.Trim().ToUpperInvariant(), dimensionValue.Trim());
            return Ok(snapshots ?? new List<RptStatisticsSnapshot>());
        }

        // 为指定众筹项目生成统计报表快照（事务）：聚合总捐赠额、已审核通过支出、净余额、捐赠笔数，一次性写入 RPT_STATISTICSSNAPSHOTS 表
        [HttpPost("generate/{projectId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GenerateProjectReport(string projectId)
        {
            var project = await _projectRepository.GetById(projectId);
            if (project == null)
                return NotFound($"未找到 ID 为 {projectId} 的众筹项目，无法生成统计报表。");

            // 聚合项目财务数据
            var totalDonation = await _donationRepository.GetTotalDonationByProject(projectId);
            var totalExpense = await _expenseRecordRepository.GetTotalApprovedExpenseByProject(projectId);
            var donationCount = await _donationRepository.GetDonationCountByProject(projectId);
            var netBalance = totalDonation - totalExpense;

            // 事务性写入统计快照（4 条指标记录）
            await _snapshotRepository.GenerateProjectReportSnapshot(
                projectId,
                totalDonation,
                totalExpense,
                netBalance,
                donationCount);

            return Ok(new
            {
                message = "项目统计报表已生成。",
                projectId,
                projectTitle = project.Title,
                metrics = new
                {
                    totalDonation,
                    totalExpense,
                    netBalance,
                    donationCount
                }
            });
        }
    }
}
