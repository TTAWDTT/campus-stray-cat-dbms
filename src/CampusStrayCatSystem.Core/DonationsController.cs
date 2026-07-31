using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    // 捐赠控制器，对应数据库表 FUND_DONATIONS
    // 记录捐赠时，系统在事务中同时把金额累加到项目的已筹金额，保证财务数据一致
    [Route("api/donations")]
    [ApiController]
    public class DonationsController : ControllerBase
    {
        private readonly IFundDonationRepository _donationRepository;
        private readonly IFundCrowdfundingProjectRepository _projectRepository;
        private readonly IReferenceCheckRepository _referenceCheck;

        public DonationsController(
            IFundDonationRepository donationRepository,
            IFundCrowdfundingProjectRepository projectRepository,
            IReferenceCheckRepository referenceCheck)
        {
            _donationRepository = donationRepository;
            _projectRepository = projectRepository;
            _referenceCheck = referenceCheck;
        }

        // 获取所有捐赠记录
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FundDonation>>> GetAll()
        {
            var donations = await _donationRepository.GetAll();
            return Ok(donations ?? new List<FundDonation>());
        }

        // 按捐赠 ID 获取单条捐赠记录
        [HttpGet("{id}")]
        public async Task<ActionResult<FundDonation>> GetById(string id)
        {
            var donation = await _donationRepository.GetById(id);
            if (donation == null)
                return NotFound($"未找到 ID 为 {id} 的捐赠记录。");

            return Ok(donation);
        }

        // 按项目查询捐赠记录
        [HttpGet("by-project/{projectId}")]
        public async Task<ActionResult<IEnumerable<FundDonation>>> GetByProject(string projectId)
        {
            if (!await _projectRepository.Exists(projectId))
                return NotFound($"未找到 ID 为 {projectId} 的众筹项目。");

            var donations = await _donationRepository.GetByProject(projectId);
            return Ok(donations ?? new List<FundDonation>());
        }

        // 按捐赠人查询其捐赠记录
        [HttpGet("by-donor/{donorUserId}")]
        public async Task<ActionResult<IEnumerable<FundDonation>>> GetByDonor(string donorUserId)
        {
            var donations = await _donationRepository.GetByDonor(donorUserId);
            return Ok(donations ?? new List<FundDonation>());
        }

        // 记录捐赠（事务）：新增捐赠记录，并把金额累加到项目已筹金额
        [HttpPost]
        public async Task<ActionResult<FundDonation>> Create([FromBody] FundDonation donation)
        {
            if (donation == null)
                return BadRequest("捐赠数据为空，无法创建。");

            var validationError = await ValidateDonation(donation);
            if (validationError != null)
                return BadRequest(validationError);

            // 事务性写入：捐赠记录 + 累加已筹金额
            await _donationRepository.CreateWithRaisedUpdate(donation);
            return CreatedAtAction(nameof(GetById), new { id = donation.DonationID }, donation);
        }

        // 业务校验：项目存在且处于可捐赠状态、捐赠人存在、金额为正、支付方式合法
        private async Task<string?> ValidateDonation(FundDonation donation)
        {
            // 项目必填且存在
            if (string.IsNullOrWhiteSpace(donation.ProjectID))
                return "ProjectID 不能为空。";

            var project = await _projectRepository.GetById(donation.ProjectID);
            if (project == null)
                return $"众筹项目 ProjectID='{donation.ProjectID}' 不存在。";

            // 项目必须处于进行中（ACTIVE）状态才接受捐赠
            if (!string.Equals(project.ProjectStatus, ProjectStatuses.Active, StringComparison.OrdinalIgnoreCase))
                return $"项目当前状态为 '{project.ProjectStatus}'，仅 '{ProjectStatuses.Active}' 状态的项目可接受捐赠。";

            // 捐赠人（若指定）必须存在
            if (!string.IsNullOrWhiteSpace(donation.DonorUserID))
            {
                if (!await _referenceCheck.UserExists(donation.DonorUserID))
                    return $"捐赠人 UserID='{donation.DonorUserID}' 不存在。";
            }

            // 金额必须为正数
            if (!donation.Amount.HasValue || donation.Amount.Value <= 0)
                return "捐赠金额 Amount 必须为正数。";

            return null; // 校验通过
        }
    }
}
