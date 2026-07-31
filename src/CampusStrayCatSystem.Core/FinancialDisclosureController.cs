using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    // 财务公示控制器
    // 数据来源：FUND_CROWDFUNDINGPROJECTS、FUND_DONATIONS、FUND_FINANCERECORDS
    [Route("api/financial-disclosure")]
    [ApiController]
    public class FinancialDisclosureController : ControllerBase
    {
        private readonly IFundCrowdfundingProjectRepository _projectRepository;
        private readonly IFundDonationRepository _donationRepository;
        private readonly IFundExpenseRecordRepository _expenseRecordRepository;

        public FinancialDisclosureController(
            IFundCrowdfundingProjectRepository projectRepository,
            IFundDonationRepository donationRepository,
            IFundExpenseRecordRepository expenseRecordRepository)
        {
            _projectRepository = projectRepository;
            _donationRepository = donationRepository;
            _expenseRecordRepository = expenseRecordRepository;
        }

        // 获取指定项目的财务公示：项目信息、已筹金额、已通过支出、净余额、捐赠明细、支出明细
        [HttpGet("{projectId}")]
        public async Task<ActionResult<FinancialDisclosureDto>> GetByProject(string projectId)
        {
            var project = await _projectRepository.GetById(projectId);
            if (project == null)
                return NotFound($"未找到 ID 为 {projectId} 的众筹项目。");

            // 查询已审核通过的支出明细
            var expenses = await _expenseRecordRepository.GetApprovedExpensesByProject(projectId);

            // 统计已审核通过的支出总额
            var totalExpense = await _expenseRecordRepository.GetTotalApprovedExpenseByProject(projectId);

            // 查询捐赠明细
            var donations = await _donationRepository.GetForDisclosureByProject(projectId);

            // 统计捐赠笔数
            var donationCount = await _donationRepository.GetDonationCountByProject(projectId);

            // 组装财务公示视图对象
            var disclosure = new FinancialDisclosureDto
            {
                Project = project,
                TotalExpense = totalExpense,
                DonationCount = donationCount,
                Donations = donations ?? new List<FundDonation>(),
                Expenses = expenses ?? new List<FundExpenseRecord>()
            };

            return Ok(disclosure);
        }

        // 获取所有进行中项目的财务公示摘要（用于公示列表页）
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<FinancialDisclosureDto>>> GetSummary()
        {
            var projects = await _projectRepository.GetByStatus(ProjectStatuses.Active);
            var result = new List<FinancialDisclosureDto>();

            foreach (var project in projects)
            {
                var totalExpense = await _expenseRecordRepository.GetTotalApprovedExpenseByProject(project.ProjectID);
                var donationCount = await _donationRepository.GetDonationCountByProject(project.ProjectID);

                result.Add(new FinancialDisclosureDto
                {
                    Project = project,
                    TotalExpense = totalExpense,
                    DonationCount = donationCount,
                    Donations = new List<FundDonation>(),
                    Expenses = new List<FundExpenseRecord>()
                });
            }

            return Ok(result);
        }
    }
}
