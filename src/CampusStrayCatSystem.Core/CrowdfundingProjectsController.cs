using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    // 众筹项目控制器，对应数据库表 FUND_CROWDFUNDINGPROJECTS
    // 提供众筹项目的创建、查询、更新、状态管理
    [Route("api/crowdfunding-projects")]
    [ApiController]
    public class CrowdfundingProjectsController : ControllerBase
    {
        private readonly IFundCrowdfundingProjectRepository _projectRepository;
        private readonly IReferenceCheckRepository _referenceCheck;

        public CrowdfundingProjectsController(
            IFundCrowdfundingProjectRepository projectRepository,
            IReferenceCheckRepository referenceCheck)
        {
            _projectRepository = projectRepository;
            _referenceCheck = referenceCheck;
        }

        // 获取所有众筹项目
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FundCrowdfundingProject>>> GetAll()
        {
            var projects = await _projectRepository.GetAll();
            return Ok(projects ?? new List<FundCrowdfundingProject>());
        }

        // 按项目 ID 获取单个众筹项目
        [HttpGet("{id}")]
        public async Task<ActionResult<FundCrowdfundingProject>> GetById(string id)
        {
            var project = await _projectRepository.GetById(id);
            if (project == null)
                return NotFound($"未找到 ID 为 {id} 的众筹项目。");

            return Ok(project);
        }

        // 按状态筛选众筹项目（如 ACTIVE 进行中、COMPLETED 已结束）
        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<IEnumerable<FundCrowdfundingProject>>> GetByStatus(string status)
        {
            if (!ProjectStatuses.IsValid(status))
                return BadRequest($"无效的项目状态 '{status}'。允许的状态: {string.Join(", ", ProjectStatuses.Allowed)}");

            var projects = await _projectRepository.GetByStatus(status);
            return Ok(projects ?? new List<FundCrowdfundingProject>());
        }

        // 按猫咪查询众筹项目
        [HttpGet("by-cat/{catId}")]
        public async Task<ActionResult<IEnumerable<FundCrowdfundingProject>>> GetByCat(string catId)
        {
            var projects = await _projectRepository.GetByCat(catId);
            return Ok(projects ?? new List<FundCrowdfundingProject>());
        }

        // 创建众筹项目
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<FundCrowdfundingProject>> Create([FromBody] FundCrowdfundingProject project)
        {
            if (project == null)
                return BadRequest("众筹项目数据为空，无法创建。");

            if (string.IsNullOrWhiteSpace(project.Title))
                return BadRequest("项目标题 Title 不能为空。");

            var validationError = await ValidateProject(project);
            if (validationError != null)
                return BadRequest(validationError);

            project.RaisedAmount = 0;
            project.ProjectStatus = string.IsNullOrWhiteSpace(project.ProjectStatus)
                ? ProjectStatuses.Active
                : project.ProjectStatus.ToUpperInvariant();
            if (await _projectRepository.Create(project) != 1)
                return Conflict("众筹项目创建未生效。");
            return CreatedAtAction(nameof(GetById), new { id = project.ProjectID }, project);
        }

        // 更新众筹项目基本信息
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(string id, [FromBody] FundCrowdfundingProject project)
        {
            if (project == null)
                return BadRequest("众筹项目数据为空，无法更新。");

            if (id != project.ProjectID)
                return BadRequest("URL 中的 ID 与请求体中的 ID 不匹配。");

            var existing = await _projectRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的众筹项目，无法更新。");

            if (string.IsNullOrWhiteSpace(project.Title))
                return BadRequest("项目标题 Title 不能为空。");

            var validationError = await ValidateProject(project);
            if (validationError != null)
                return BadRequest(validationError);

            return await _projectRepository.Update(project) == 1
                ? NoContent()
                : Conflict("众筹项目更新未生效。");
        }

        // 更新项目状态（如发布为 ACTIVE、结束为 COMPLETED）
        [HttpPut("{id}/status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateProjectStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewStatus))
                return BadRequest("新状态不能为空。");

            if (!ProjectStatuses.IsValid(request.NewStatus))
                return BadRequest($"无效的项目状态 '{request.NewStatus}'。允许的状态: {string.Join(", ", ProjectStatuses.Allowed)}");

            var existing = await _projectRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的众筹项目，无法更新状态。");

            return await _projectRepository.UpdateStatus(id, request.NewStatus.ToUpperInvariant()) == 1
                ? Ok(new { message = "众筹项目状态更新成功。" })
                : Conflict("众筹项目状态更新未生效。");
        }

        // 业务校验：猫咪存在性、金额非负、时间先后、状态合法性
        private async Task<string?> ValidateProject(FundCrowdfundingProject project)
        {
            // 猫咪（若指定）必须存在
            if (!string.IsNullOrWhiteSpace(project.CatID))
            {
                if (!await _referenceCheck.CatExists(project.CatID))
                    return $"猫咪 CatID='{project.CatID}' 不存在。";
            }

            // 目标金额不能为负
            if (project.TargetAmount.HasValue && project.TargetAmount.Value < 0)
                return "目标金额 TargetAmount 不能为负数。";

            // 状态合法性
            if (!string.IsNullOrWhiteSpace(project.ProjectStatus))
            {
                if (!ProjectStatuses.IsValid(project.ProjectStatus))
                    return $"无效的项目状态 '{project.ProjectStatus}'。允许的状态: {string.Join(", ", ProjectStatuses.Allowed)}";
            }

            // 开始时间不能晚于结束时间
            if (project.StartTime.HasValue && project.EndTime.HasValue)
            {
                if (project.EndTime.Value < project.StartTime.Value)
                    return "项目结束时间不能早于开始时间。";
            }

            return null; // 校验通过
        }
    }

    // 更新众筹项目状态的请求体
    public class UpdateProjectStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty; // 新状态（ACTIVE/COMPLETED/CANCELLED）
    }
}
