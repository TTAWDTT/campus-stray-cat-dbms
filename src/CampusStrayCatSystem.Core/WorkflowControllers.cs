using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core
{
    // 公开领养流程接口，直接把请求转给数据库包。
    [Route("api/adoption-workflow")]
    [ApiController]
    public class AdoptionWorkflowController : ControllerBase
    {
        private readonly IAdoptionWorkflowRepository _repository;

        public AdoptionWorkflowController(IAdoptionWorkflowRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<AdoptionPendingAppDto>>> GetPendingApplications()
        {
            // 前端待审核列表入口。
            return Ok(await _repository.GetPendingApplicationsAsync());
        }

        [HttpGet("visits")]
        public async Task<ActionResult<IEnumerable<AdoptionVisitSummaryDto>>> GetVisitSummary()
        {
            return Ok(await _repository.GetVisitSummaryAsync());
        }

        [HttpPost("applications")]
        public async Task<IActionResult> SubmitApplication([FromBody] AdoptionApplicationCreateRequest request)
        {
            await _repository.SubmitApplicationAsync(request);
            return Ok();
        }

        [HttpPost("applications/{applicationId}/review")]
        public async Task<IActionResult> ReviewApplication(string applicationId, [FromBody] AdoptionApplicationReviewRequest request)
        {
            await _repository.ReviewApplicationAsync(applicationId, request);
            return NoContent();
        }

        [HttpPost("applications/{applicationId}/visits")]
        public async Task<IActionResult> CreateVisit(string applicationId, [FromBody] AdoptionVisitCreateRequest request)
        {
            await _repository.CreateVisitAsync(applicationId, request);
            return Ok();
        }
    }

    // 公开志愿者流程接口，直接把请求转给数据库包。
    [Route("api/volunteer-workflow")]
    [ApiController]
    public class VolunteerWorkflowController : ControllerBase
    {
        private readonly IVolunteerWorkflowRepository _repository;

        public VolunteerWorkflowController(IVolunteerWorkflowRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("activity")]
        public async Task<ActionResult<IEnumerable<VolunteerActivityDto>>> GetActivity()
        {
            // 前端志愿者看板入口。
            return Ok(await _repository.GetActivityAsync());
        }

        [HttpPost("volunteers")]
        public async Task<IActionResult> RegisterVolunteer([FromBody] VolunteerRegisterRequest request)
        {
            await _repository.RegisterVolunteerAsync(request);
            return Ok();
        }

        [HttpPost("shifts")]
        public async Task<IActionResult> CreateShift([FromBody] VolunteerShiftCreateRequest request)
        {
            await _repository.CreateShiftAsync(request);
            return Ok();
        }

        [HttpPost("shifts/{shiftId}/checkins")]
        public async Task<IActionResult> CheckInShift(string shiftId, [FromBody] VolunteerCheckInRequest request)
        {
            await _repository.CheckInShiftAsync(shiftId, request);
            return Ok();
        }

        [HttpPost("credit-logs")]
        public async Task<IActionResult> AddCreditLog([FromBody] VolunteerCreditLogCreateRequest request)
        {
            await _repository.AddCreditLogAsync(request);
            return Ok();
        }
    }
}