using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;
using System.Security.Claims;

namespace CampusStrayCatSystem.Core
{
    // 投喂任务控制器，对应数据库表 VOL_SHIFTS
    // 提供查看所有任务、按志愿者/点位/状态筛选、创建排班、更新任务、更新状态
    [Route("api/feeding-tasks")]
    [ApiController]
    [Authorize(Roles = "ADMIN,VOLUNTEER")]
    public class FeedingTasksController : ControllerBase
    {
        private readonly IVolShiftRepository _shiftRepository;
        private readonly IReferenceCheckRepository _referenceCheck;

        public FeedingTasksController(
            IVolShiftRepository shiftRepository,
            IReferenceCheckRepository referenceCheck)
        {
            _shiftRepository = shiftRepository;
            _referenceCheck = referenceCheck;
        }

        // 获取所有投喂任务（按计划开始时间倒序）
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VolShift>>> GetAll()
        {
            var shifts = await _shiftRepository.GetAll();
            return Ok(shifts ?? new List<VolShift>());
        }

        // 按任务 ID 获取单个投喂任务
        [HttpGet("{id}")]
        public async Task<ActionResult<VolShift>> GetById(string id)
        {
            var shift = await _shiftRepository.GetById(id);
            if (shift == null)
                return NotFound($"未找到 ID 为 {id} 的投喂任务。");

            return Ok(shift);
        }

        // 按志愿者ID查询投喂任务（“我的投喂任务”）
        [HttpGet("by-volunteer/{volunteerId}")]
        public async Task<ActionResult<IEnumerable<VolShift>>> GetByVolunteer(string volunteerId)
        {
            var shifts = await _shiftRepository.GetByVolunteer(volunteerId);
            return Ok(shifts ?? new List<VolShift>());
        }

        // 按投喂点ID查询投喂任务
        [HttpGet("by-point/{pointId}")]
        public async Task<ActionResult<IEnumerable<VolShift>>> GetByPoint(string pointId)
        {
            var shifts = await _shiftRepository.GetByPoint(pointId);
            return Ok(shifts ?? new List<VolShift>());
        }

        // 按状态筛选投喂任务（如 ASSIGNED 待执行、COMPLETED 已完成）
        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<IEnumerable<VolShift>>> GetByStatus(string status)
        {
            if (!ShiftStatuses.IsValid(status))
                return BadRequest($"无效的任务状态 '{status}'。允许的状态: {string.Join(", ", ShiftStatuses.Allowed)}");

            var shifts = await _shiftRepository.GetByStatus(status);
            return Ok(shifts ?? new List<VolShift>());
        }

        // 创建新的投喂任务
        [HttpPost]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<VolShift>> Create([FromBody] VolShift shift)
        {
            if (shift == null)
                return BadRequest("投喂任务数据为空，无法创建。");

            var validationError = await ValidateShift(shift);
            if (validationError != null)
                return BadRequest(validationError);

            if (!User.IsInRole("ADMIN") && !await IsCurrentVolunteerAsync(shift.VolunteerID))
                return Forbid();

            if (await _shiftRepository.Create(shift) != 1)
                return Conflict("投喂任务创建未生效。");
            return CreatedAtAction(nameof(GetById), new { id = shift.ShiftID }, shift);
        }

        // 更新投喂任务基本信息
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> Update(string id, [FromBody] VolShift shift)
        {
            if (shift == null)
                return BadRequest("投喂任务数据为空，无法更新。");

            if (id != shift.ShiftID)
                return BadRequest("URL 中的 ID 与请求体中的 ID 不匹配。");

            var existing = await _shiftRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的投喂任务，无法更新。");

            var validationError = await ValidateShift(shift);
            if (validationError != null)
                return BadRequest(validationError);

            if (!User.IsInRole("ADMIN") && !await IsCurrentVolunteerAsync(existing.VolunteerID))
                return Forbid();

            return await _shiftRepository.Update(shift) == 1
                ? NoContent()
                : Conflict("投喂任务更新未生效。");
        }

        // 更新投喂任务状态（如标记为已完成/爽约）
        [HttpPut("{id}/status")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateShiftStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewStatus))
                return BadRequest("新状态不能为空。");

            if (!ShiftStatuses.IsValid(request.NewStatus))
                return BadRequest($"无效的任务状态 '{request.NewStatus}'。允许的状态: {string.Join(", ", ShiftStatuses.Allowed)}");

            var existing = await _shiftRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的投喂任务，无法更新状态。");

            if (!User.IsInRole("ADMIN") && !await IsCurrentVolunteerAsync(existing.VolunteerID))
                return Forbid();

            return await _shiftRepository.UpdateStatus(id, request.NewStatus.ToUpperInvariant()) == 1
                ? Ok(new { message = "投喂任务状态更新成功。" })
                : Conflict("投喂任务状态更新未生效。");
        }

        // 业务校验：志愿者、投喂点存在性；状态合法性；时间先后顺序
        private async Task<string?> ValidateShift(VolShift shift)
        {
            // 志愿者必填且必须存在
            if (string.IsNullOrWhiteSpace(shift.VolunteerID))
                return "VolunteerID 不能为空。";

            if (!await _referenceCheck.VolunteerExists(shift.VolunteerID))
                return $"志愿者 VolunteerID='{shift.VolunteerID}' 不存在。";

            // 投喂点（若指定）必须存在
            if (!string.IsNullOrWhiteSpace(shift.PointID))
            {
                if (!await _referenceCheck.ServicePointExists(shift.PointID))
                    return $"投喂点 PointID='{shift.PointID}' 不存在。";
            }

            if (!string.IsNullOrWhiteSpace(shift.BackupVolunteerID))
            {
                if (string.Equals(shift.VolunteerID, shift.BackupVolunteerID, StringComparison.OrdinalIgnoreCase))
                    return "备用志愿者不能与负责志愿者相同。";

                if (!await _referenceCheck.VolunteerExists(shift.BackupVolunteerID))
                    return $"备用志愿者 VolunteerID='{shift.BackupVolunteerID}' 不存在。";
            }

            // 状态合法性
            if (!string.IsNullOrWhiteSpace(shift.ShiftStatus))
            {
                if (!ShiftStatuses.IsValid(shift.ShiftStatus))
                    return $"无效的任务状态 '{shift.ShiftStatus}'。允许的状态: {string.Join(", ", ShiftStatuses.Allowed)}";
            }

            // 计划开始时间不能晚于计划结束时间
            if (shift.PlanStartTime.HasValue && shift.PlanEndTime.HasValue)
            {
                if (shift.PlanEndTime.Value < shift.PlanStartTime.Value)
                    return "计划结束时间不能早于计划开始时间。";
            }

            return null; // 校验通过
        }

        private async Task<bool> IsCurrentVolunteerAsync(string volunteerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return false;

            var volunteerUserId = await _referenceCheck.GetVolunteerUserId(volunteerId);
            return string.Equals(userId, volunteerUserId, StringComparison.OrdinalIgnoreCase);
        }
    }

    // 更新投喂任务状态的请求体
    public class UpdateShiftStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
    }
}
