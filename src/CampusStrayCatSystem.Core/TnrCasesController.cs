using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class TnrCasesController : ControllerBase
    {
        private readonly ITnrCaseRepository _tnrCaseRepository;
        private readonly ITnrStatusLogRepository _tnrStatusLogRepository;
        private readonly ICatRepository _catRepository;
        private readonly IUserRepository _userRepository;

        public TnrCasesController(
            ITnrCaseRepository tnrCaseRepository,
            ITnrStatusLogRepository tnrStatusLogRepository,
            ICatRepository catRepository,
            IUserRepository userRepository)
        {
            _tnrCaseRepository = tnrCaseRepository;
            _tnrStatusLogRepository = tnrStatusLogRepository;
            _catRepository = catRepository;
            _userRepository = userRepository;
        }

        //获取所有TNR案例
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TnrCase>>> GetTnrCases()
        {
            var cases = await _tnrCaseRepository.GetAll();
            return Ok(cases ?? new List<TnrCase>());
        }

        //根据ID获取单个TNR案例
        [HttpGet("{id}")]
        public async Task<ActionResult<TnrCase>> GetTnrCase(string id)
        {
            var tnrCase = await _tnrCaseRepository.GetById(id);

            if (tnrCase == null)
                return NotFound($"未找到 ID 为 {id} 的TNR案例。");

            return Ok(tnrCase);
        }

        //创建新的TNR案例
        [HttpPost]
        public async Task<ActionResult<TnrCase>> CreateTnrCase([FromBody] TnrCase tnrCase)
        {
            if (tnrCase == null)
                return BadRequest("TNR案例数据为空，无法创建。");

            // 业务校验
            var validationError = await ValidateTnrCase(tnrCase);
            if (validationError != null)
                return BadRequest(validationError);

            await _tnrCaseRepository.Create(tnrCase);
            return CreatedAtAction(nameof(GetTnrCase), new { id = tnrCase.CaseID }, tnrCase);
        }

        //更新TNR案例基本信息
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTnrCase(string id, [FromBody] TnrCase tnrCase)
        {
            if (tnrCase == null)
                return BadRequest("TNR案例数据为空，无法更新。");

            if (id != tnrCase.CaseID)
                return BadRequest("URL 中的 ID 与请求体中的 ID 不匹配。");

            var existing = await _tnrCaseRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的TNR案例，无法更新。");

            // 业务校验
            var validationError = await ValidateTnrCase(tnrCase);
            if (validationError != null)
                return BadRequest(validationError);

            await _tnrCaseRepository.Update(tnrCase);
            return NoContent();
        }

        //更新TNR状态（自动生成状态流转日志，同一事务）
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTnrStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewStatus))
                return BadRequest("新状态不能为空。");

            // 校验状态值是否合法
            if (!TnrStatuses.IsValid(request.NewStatus))
                return BadRequest($"无效的状态值 '{request.NewStatus}'。允许的状态: {string.Join(", ", TnrStatuses.Allowed)}");

            var existing = await _tnrCaseRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的TNR案例，无法更新状态。");

            var oldStatus = existing.CurrentStatus;

            // 在同一事务中更新状态并生成日志
            await _tnrCaseRepository.UpdateStatusWithLog(id, request.NewStatus, oldStatus ?? "", request.OperatorID, request.Remark);

            return Ok(new { oldStatus, newStatus = request.NewStatus, message = "状态更新成功，已生成流转日志。" });
        }

        // 业务校验
        private async Task<string?> ValidateTnrCase(TnrCase tnrCase)
        {
            // CatID 非空且猫咪存在
            if (string.IsNullOrWhiteSpace(tnrCase.CatID))
                return "CatID 不能为空。";

            if (!await _catRepository.Exists(tnrCase.CatID))
                return $"猫咪 CatID='{tnrCase.CatID}' 不存在。";

            // 负责人存在（如果指定了）
            if (!string.IsNullOrWhiteSpace(tnrCase.ResponsibleUserID))
            {
                if (!await _userRepository.Exists(tnrCase.ResponsibleUserID))
                    return $"负责人 UserID='{tnrCase.ResponsibleUserID}' 不存在。";
            }

            // 状态值合法（如果指定了）
            if (!string.IsNullOrWhiteSpace(tnrCase.CurrentStatus))
            {
                if (!TnrStatuses.IsValid(tnrCase.CurrentStatus))
                    return $"无效的状态值 '{tnrCase.CurrentStatus}'。允许的状态: {string.Join(", ", TnrStatuses.Allowed)}";
            }

            // TotalCost >= 0
            if (tnrCase.TotalCost.HasValue && tnrCase.TotalCost.Value < 0)
                return "TotalCost 不能为负数。";

            // CaptureTime <= SurgeryTime <= ReleaseTime
            if (tnrCase.CaptureTime.HasValue && tnrCase.SurgeryTime.HasValue)
            {
                if (tnrCase.SurgeryTime.Value < tnrCase.CaptureTime.Value)
                    return "手术时间不能早于捕获时间。";
            }

            if (tnrCase.SurgeryTime.HasValue && tnrCase.ReleaseTime.HasValue)
            {
                if (tnrCase.ReleaseTime.Value < tnrCase.SurgeryTime.Value)
                    return "释放时间不能早于手术时间。";
            }

            if (tnrCase.CaptureTime.HasValue && tnrCase.ReleaseTime.HasValue)
            {
                if (tnrCase.ReleaseTime.Value < tnrCase.CaptureTime.Value)
                    return "释放时间不能早于捕获时间。";
            }

            return null; // 校验通过
        }
    }

    //状态更新请求体
    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? OperatorID { get; set; }
        public string? Remark { get; set; }
    }
}
