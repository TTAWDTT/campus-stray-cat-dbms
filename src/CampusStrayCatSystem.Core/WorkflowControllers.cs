using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CampusStrayCatSystem.Core
{
    // 公开领养流程接口，直接把请求转给数据库包。
    [Route("api/adoption-workflow")]
    [ApiController]
    [Authorize]
    public class AdoptionWorkflowController : ControllerBase
    {
        private readonly IAdoptionWorkflowRepository _repository;

        public AdoptionWorkflowController(IAdoptionWorkflowRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("pending")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<IEnumerable<AdoptionPendingAppDto>>> GetPendingApplications()
        {
            // 前端待审核列表入口。
            return Ok(await _repository.GetPendingApplicationsAsync());
        }

        [HttpGet("applications")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<IEnumerable<AdoptionPendingAppDto>>> GetApplications([FromQuery] string? status = "APPROVED")
        {
            var targetStatus = string.IsNullOrWhiteSpace(status) ? "APPROVED" : status.Trim().ToUpperInvariant();
            if (targetStatus is not ("PENDING" or "APPROVED" or "REJECTED"))
                return BadRequest(new { message = "status 只能是 PENDING、APPROVED 或 REJECTED。" });

            return Ok(await _repository.GetApplicationsByStatusAsync(targetStatus));
        }

        [HttpGet("visits")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<IEnumerable<AdoptionVisitSummaryDto>>> GetVisitSummary()
        {
            return Ok(await _repository.GetVisitSummaryAsync());
        }

        [HttpPost("applications")]
        public async Task<IActionResult> SubmitApplication([FromBody] AdoptionApplicationCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CatId))
                return BadRequest(new { message = "CatId 不能为空。" });

            var userId = CurrentUserId();
            if (userId == null) return Unauthorized();
            request.ApplicantUserId = userId;

            await _repository.SubmitApplicationAsync(request);
            return Ok();
        }

        [HttpPost("applications/{applicationId}/review")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> ReviewApplication(string applicationId, [FromBody] AdoptionApplicationReviewRequest request)
        {
            if (string.IsNullOrWhiteSpace(applicationId) || request == null)
                return BadRequest(new { message = "申请编号和审核内容不能为空。" });
            if (request.Status is not ("APPROVED" or "REJECTED"))
                return BadRequest(new { message = "Status 只能是 APPROVED 或 REJECTED。" });

            var userId = CurrentUserId();
            if (userId == null) return Unauthorized();
            request.ReviewerUserId = userId;

            await _repository.ReviewApplicationAsync(applicationId, request);
            return NoContent();
        }

        [HttpPost("applications/{applicationId}/visits")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> CreateVisit(string applicationId, [FromBody] AdoptionVisitCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(applicationId) || request == null || string.IsNullOrWhiteSpace(request.VisitType))
                return BadRequest(new { message = "申请编号和回访类型不能为空。" });
            if (!VisitTypes.IsValid(request.VisitType))
                return BadRequest(new { message = "VisitType 只能是 INITIAL、FOLLOW_UP 或 FINAL。" });
            if (request.PassFlag is not (0 or 1))
                return BadRequest(new { message = "PassFlag 只能是 0 或 1。" });

            request.VisitType = request.VisitType.Trim().ToUpperInvariant();

            var userId = CurrentUserId();
            if (userId == null) return Unauthorized();
            request.VisitorUserId = userId;

            await _repository.CreateVisitAsync(applicationId, request);
            return Ok();
        }

        private string? CurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        }
    }

    // 公开志愿者流程接口，直接把请求转给数据库包。
    [Route("api/volunteer-workflow")]
    [ApiController]
    [Authorize]
    public class VolunteerWorkflowController : ControllerBase
    {
        private readonly IVolunteerWorkflowRepository _repository;

        public VolunteerWorkflowController(IVolunteerWorkflowRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("activity")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<IEnumerable<VolunteerActivityDto>>> GetActivity()
        {
            // 前端志愿者看板入口。
            return Ok(await _repository.GetActivityAsync());
        }

        [HttpPost("volunteers")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RegisterVolunteer([FromBody] VolunteerRegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "UserId 不能为空。" });

            await _repository.RegisterVolunteerAsync(request);
            return Ok();
        }

        [HttpPost("shifts")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateShift([FromBody] VolunteerShiftCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.VolunteerId) || string.IsNullOrWhiteSpace(request.PointId))
                return BadRequest(new { message = "VolunteerId 和 PointId 不能为空。" });
            if (request.PlanEndTime <= request.PlanStartTime)
                return BadRequest(new { message = "排班结束时间必须晚于开始时间。" });

            await _repository.CreateShiftAsync(request);
            return Ok();
        }

        [HttpPost("shifts/{shiftId}/checkins")]
        [Authorize(Roles = "VOLUNTEER")]
        public async Task<IActionResult> CheckInShift(string shiftId, [FromBody] VolunteerCheckInRequest request)
        {
            if (string.IsNullOrWhiteSpace(shiftId) || request == null)
                return BadRequest(new { message = "排班编号和签到内容不能为空。" });

            var userId = CurrentUserId();
            if (userId == null) return Unauthorized();
            await _repository.CheckInShiftAsync(shiftId, request, userId);
            return Ok();
        }

        [HttpPost("credit-logs")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AddCreditLog([FromBody] VolunteerCreditLogCreateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.VolunteerId) || string.IsNullOrWhiteSpace(request.SourceType))
                return BadRequest(new { message = "积分记录缺少必要字段。" });

            await _repository.AddCreditLogAsync(request);
            return Ok();
        }

        private string? CurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        }
    }
}
