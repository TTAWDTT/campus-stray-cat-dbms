using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    // 支出记录控制器，对应数据库表 FUND_FINANCERECORDS
    // 提供支出的记录、查询、审核功能，支出需审核通过后才计入财务公示
    [Route("api/expense-records")]
    [ApiController]
    [Authorize(Roles = "ADMIN,VOLUNTEER")]
    public class ExpenseRecordsController : ControllerBase
    {
        private readonly IFundExpenseRecordRepository _expenseRecordRepository;
        private readonly IFundCrowdfundingProjectRepository _projectRepository;
        private readonly IReferenceCheckRepository _referenceCheck;

        public ExpenseRecordsController(
            IFundExpenseRecordRepository expenseRecordRepository,
            IFundCrowdfundingProjectRepository projectRepository,
            IReferenceCheckRepository referenceCheck)
        {
            _expenseRecordRepository = expenseRecordRepository;
            _projectRepository = projectRepository;
            _referenceCheck = referenceCheck;
        }

        // 获取所有支出记录
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FundExpenseRecord>>> GetAll()
        {
            var records = await _expenseRecordRepository.GetAll();
            return Ok(records ?? new List<FundExpenseRecord>());
        }

        // 按支出 ID 获取单条支出记录
        [HttpGet("{id}")]
        public async Task<ActionResult<FundExpenseRecord>> GetById(string id)
        {
            var record = await _expenseRecordRepository.GetById(id);
            if (record == null)
                return NotFound($"未找到 ID 为 {id} 的支出记录。");

            return Ok(record);
        }

        // 按项目查询支出记录
        [HttpGet("by-project/{projectId}")]
        public async Task<ActionResult<IEnumerable<FundExpenseRecord>>> GetByProject(string projectId)
        {
            if (!await _projectRepository.Exists(projectId))
                return NotFound($"未找到 ID 为 {projectId} 的众筹项目。");

            var records = await _expenseRecordRepository.GetByProject(projectId);
            return Ok(records ?? new List<FundExpenseRecord>());
        }

        // 按项目查询已审核通过的支出记录（用于财务公示）
        [HttpGet("by-project/{projectId}/approved-expenses")]
        public async Task<ActionResult<IEnumerable<FundExpenseRecord>>> GetApprovedExpenses(string projectId)
        {
            if (!await _projectRepository.Exists(projectId))
                return NotFound($"未找到 ID 为 {projectId} 的众筹项目。");

            var records = await _expenseRecordRepository.GetApprovedExpensesByProject(projectId);
            return Ok(records ?? new List<FundExpenseRecord>());
        }

        // 记录支出：新增一条支出记录（默认待审核状态）
        [HttpPost]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<FundExpenseRecord>> Create([FromBody] FundExpenseRecord record)
        {
            if (record == null)
                return BadRequest("支出记录数据为空，无法创建。");

            var validationError = await ValidateExpenseRecord(record);
            if (validationError != null)
                return BadRequest(validationError);

            // 审核字段由服务器维护，创建接口不得绕过审核流程。
            record.AuditUserID = null;
            record.AuditStatus = AuditStatuses.Pending;
            record.PublicTime = null;
            if (await _expenseRecordRepository.Create(record) != 1)
                return Conflict("支出记录创建未生效。");
            return CreatedAtAction(nameof(GetById), new { id = record.FinanceID }, record);
        }

        // 审核支出记录：管理员审核支出，更新审核状态和审核人，审核通过后会记录公示时间
        [HttpPut("{id}/audit")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Audit(string id, [FromBody] AuditExpenseRecordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AuditStatus))
                return BadRequest("审核状态不能为空。");

            var normalizedStatus = request.AuditStatus.ToUpperInvariant();
            if (normalizedStatus is not (AuditStatuses.Approved or AuditStatuses.Rejected))
                return BadRequest("审核状态只能是 APPROVED 或 REJECTED。");

            var auditUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(auditUserId))
                return Unauthorized();

            var existing = await _expenseRecordRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的支出记录，无法审核。");

            // 只有待审核状态的记录才能被审核
            if (!string.Equals(existing.AuditStatus, AuditStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"当前审核状态为 '{existing.AuditStatus}'，仅 '{AuditStatuses.Pending}' 状态的记录可审核。");

            // 校验审核人存在
            if (!await _referenceCheck.UserExists(auditUserId))
                return Unauthorized();

            var updated = await _expenseRecordRepository.Audit(id, auditUserId, normalizedStatus);
            if (updated != 1)
                return Conflict("支出记录审核未生效。");
            if (updated != 1)
                return Conflict("支出记录的审核状态已经变化，请刷新后重试。");

            return Ok(new { message = "支出记录审核完成。" });
        }

        // 业务校验：项目存在、金额为正、审核状态合法
        private async Task<string?> ValidateExpenseRecord(FundExpenseRecord record)
        {
            // 项目必填且存在
            if (string.IsNullOrWhiteSpace(record.ProjectID))
                return "ProjectID 不能为空。";

            if (!await _projectRepository.Exists(record.ProjectID))
                return $"众筹项目 ProjectID='{record.ProjectID}' 不存在。";

            // 金额必须为正数
            if (!record.Amount.HasValue || record.Amount.Value <= 0)
                return "金额 Amount 必须为正数。";

            return null; // 校验通过
        }
    }

    // 审核支出记录的请求体
    public class AuditExpenseRecordRequest
    {
        public string AuditUserID { get; set; } = string.Empty;  // 审核人 ID
        public string AuditStatus { get; set; } = string.Empty;  // 审核状态（APPROVED/REJECTED）
    }
}
