using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;
using System.Security.Claims;

namespace CampusStrayCatSystem.Core
{
    // 志愿者交接控制器，对应数据库表 VOL_HANDOVERS
    [Route("api/handovers")]
    [ApiController]
    [Authorize(Roles = "ADMIN,VOLUNTEER")]
    public class HandoversController : ControllerBase
    {
        private readonly IVolHandoverRepository _handoverRepository;
        private readonly IReferenceCheckRepository _referenceCheck;
        private readonly IVolShiftRepository _shiftRepository;

        public HandoversController(
            IVolHandoverRepository handoverRepository,
            IReferenceCheckRepository referenceCheck,
            IVolShiftRepository shiftRepository)
        {
            _handoverRepository = handoverRepository;
            _referenceCheck = referenceCheck;
            _shiftRepository = shiftRepository;
        }

        // 获取所有交接记录（按发起时间倒序）
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VolHandover>>> GetAll()
        {
            var handovers = await _handoverRepository.GetAll();
            return Ok(handovers ?? new List<VolHandover>());
        }

        // 按交接 ID 获取单条交接记录
        [HttpGet("{id}")]
        public async Task<ActionResult<VolHandover>> GetById(string id)
        {
            var handover = await _handoverRepository.GetById(id);
            if (handover == null)
                return NotFound($"未找到 ID 为 {id} 的交接记录。");

            return Ok(handover);
        }

        // 查询某志愿者发起的所有交接（“我发起的交接”）
        [HttpGet("by-from/{fromVolunteerId}")]
        public async Task<ActionResult<IEnumerable<VolHandover>>> GetByFromVolunteer(string fromVolunteerId)
        {
            var handovers = await _handoverRepository.GetByFromVolunteer(fromVolunteerId);
            return Ok(handovers ?? new List<VolHandover>());
        }

        // 查询某志愿者需要确认的所有交接（“待我确认的交接”）
        [HttpGet("by-to/{toVolunteerId}")]
        public async Task<ActionResult<IEnumerable<VolHandover>>> GetByToVolunteer(string toVolunteerId)
        {
            var handovers = await _handoverRepository.GetByToVolunteer(toVolunteerId);
            return Ok(handovers ?? new List<VolHandover>());
        }

        // 按状态筛选交接记录（交接状态可查询）
        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<IEnumerable<VolHandover>>> GetByStatus(string status)
        {
            if (!HandoverStatuses.IsValid(status))
                return BadRequest($"无效的交接状态 '{status}'。允许的状态: {string.Join(", ", HandoverStatuses.Allowed)}");

            var handovers = await _handoverRepository.GetByStatus(status.ToUpperInvariant());
            return Ok(handovers ?? new List<VolHandover>());
        }

        // 按关联对象查询交接记录（如查询某投喂任务的交接历史）
        [HttpGet("by-related/{relatedType}/{relatedId}")]
        public async Task<ActionResult<IEnumerable<VolHandover>>> GetByRelated(string relatedType, string relatedId)
        {
            var handovers = await _handoverRepository.GetByRelated(relatedType, relatedId);
            return Ok(handovers ?? new List<VolHandover>());
        }

        // 提交交接：新建交接记录，状态默认 PENDING，发起方把任务转交给接收方
        [HttpPost]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<VolHandover>> Create([FromBody] VolHandover handover)
        {
            if (handover == null)
                return BadRequest("交接数据为空，无法创建。");

            var validationError = await ValidateHandover(handover);
            if (validationError != null)
                return BadRequest(validationError);

            var access = await EnsureVolunteerAsync(handover.FromVolunteerID);
            if (access != null) return access;

            await _handoverRepository.Create(handover);
            return CreatedAtAction(nameof(GetById), new { id = handover.HandoverID }, handover);
        }

        // 确认交接：接收方接受，状态置为 CONFIRMED；若关联投喂任务，则把任务负责人改为接收方
        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> Confirm(string id)
        {
            var existing = await _handoverRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的交接记录，无法确认。");

            // 只有处于待确认状态的交接才能被确认
            if (!string.Equals(existing.HandoverStatus, HandoverStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"当前交接状态为 '{existing.HandoverStatus}'，仅处于 '{HandoverStatuses.Pending}' 状态的交接可确认。");

            var access = await EnsureVolunteerAsync(existing.ToVolunteerID);
            if (access != null) return access;

            var confirmed = await _handoverRepository.Confirm(
                id,
                existing.FromVolunteerID,
                existing.ToVolunteerID,
                existing.RelatedType,
                existing.RelatedID);
            if (!confirmed)
                return Conflict("交接状态或任务负责人已经变化，请刷新后重试。");

            return Ok(new { message = "交接已确认，关联的投喂任务负责人已更新。" });
        }

        // 拒绝交接：接收方拒绝接受
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> Reject(string id)
        {
            var existing = await _handoverRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的交接记录，无法拒绝。");

            if (!string.Equals(existing.HandoverStatus, HandoverStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"当前交接状态为 '{existing.HandoverStatus}'，仅处于 '{HandoverStatuses.Pending}' 状态的交接可拒绝。");

            var access = await EnsureVolunteerAsync(existing.ToVolunteerID);
            if (access != null) return access;

            var rejected = await _handoverRepository.Reject(id);
            if (rejected != 1)
                return Conflict("交接状态已经变化，请刷新后重试。");

            return Ok(new { message = "交接已拒绝。" });
        }

        // 撤销交接：发起方撤销尚未确认的交接
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> Cancel(string id)
        {
            var existing = await _handoverRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的交接记录，无法撤销。");

            if (!string.Equals(existing.HandoverStatus, HandoverStatuses.Pending, StringComparison.OrdinalIgnoreCase))
                return BadRequest($"当前交接状态为 '{existing.HandoverStatus}'，仅处于 '{HandoverStatuses.Pending}' 状态的交接可撤销。");

            var access = await EnsureVolunteerAsync(existing.FromVolunteerID);
            if (access != null) return access;

            var cancelled = await _handoverRepository.Cancel(id);
            if (cancelled != 1)
                return Conflict("交接状态已经变化，请刷新后重试。");

            return Ok(new { message = "交接已撤销。" });
        }

        // 业务校验：发起方与接收方必须存在且不能相同；交接类型合法；若关联投喂任务，则任务必须存在
        private async Task<string?> ValidateHandover(VolHandover handover)
        {
            // 发起方志愿者必填且存在
            if (string.IsNullOrWhiteSpace(handover.FromVolunteerID))
                return "FromVolunteerID 不能为空。";

            if (!await _referenceCheck.VolunteerExists(handover.FromVolunteerID))
                return $"发起方志愿者 VolunteerID='{handover.FromVolunteerID}' 不存在。";

            // 接收方志愿者必填且存在
            if (string.IsNullOrWhiteSpace(handover.ToVolunteerID))
                return "ToVolunteerID 不能为空。";

            if (!await _referenceCheck.VolunteerExists(handover.ToVolunteerID))
                return $"接收方志愿者 VolunteerID='{handover.ToVolunteerID}' 不存在。";

            // 发起方与接收方不能为同一人
            if (string.Equals(handover.FromVolunteerID, handover.ToVolunteerID, StringComparison.OrdinalIgnoreCase))
                return "发起方与接收方不能为同一志愿者。";

            if (!string.Equals(handover.RelatedType, "SHIFT", StringComparison.OrdinalIgnoreCase))
                return "投喂交接的 RelatedType 必须是 SHIFT。";

            if (string.IsNullOrWhiteSpace(handover.RelatedID))
                return "投喂交接的 RelatedID 不能为空。";

            var shift = await _shiftRepository.GetById(handover.RelatedID);
            if (shift == null)
                return $"关联的投喂任务 ShiftID='{handover.RelatedID}' 不存在。";

            if (!string.Equals(shift.VolunteerID, handover.FromVolunteerID, StringComparison.OrdinalIgnoreCase))
                return "只有当前任务负责人才能发起该投喂任务的交接。";

            return null; // 校验通过
        }

        private async Task<ActionResult?> EnsureVolunteerAsync(string volunteerId)
        {
            if (User.IsInRole("ADMIN")) return null;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var volunteerUserId = await _referenceCheck.GetVolunteerUserId(volunteerId);
            return string.Equals(userId, volunteerUserId, StringComparison.OrdinalIgnoreCase)
                ? null
                : Forbid();
        }
    }
}
