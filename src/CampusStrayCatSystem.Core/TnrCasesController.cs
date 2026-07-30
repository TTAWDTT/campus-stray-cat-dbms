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

        public TnrCasesController(
            ITnrCaseRepository tnrCaseRepository,
            ITnrStatusLogRepository tnrStatusLogRepository)
        {
            _tnrCaseRepository = tnrCaseRepository;
            _tnrStatusLogRepository = tnrStatusLogRepository;
        }

        //获取所有TNR案例
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TnrCase>>> GetTnrCases()
        {
            var cases = await _tnrCaseRepository.GetAll();

            if (cases == null || !cases.Any())
                return NotFound("未找到任何TNR案例数据。");

            return Ok(cases);
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

            await _tnrCaseRepository.Update(tnrCase);
            return NoContent();
        }

        //更新TNR状态（自动生成状态流转日志）
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTnrStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewStatus))
                return BadRequest("新状态不能为空。");

            var existing = await _tnrCaseRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的TNR案例，无法更新状态。");

            var oldStatus = existing.CurrentStatus;

            // 更新状态
            existing.CurrentStatus = request.NewStatus;
            existing.ResponsibleUserID = request.OperatorID ?? existing.ResponsibleUserID;
            await _tnrCaseRepository.Update(existing);

            // 自动生成状态流转日志
            await _tnrStatusLogRepository.Create(new TnrStatusLog
            {
                CaseID = id,
                FromStatus = oldStatus,
                ToStatus = request.NewStatus,
                OperatorID = request.OperatorID,
                Remark = request.Remark
            });

            return Ok(new { oldStatus, newStatus = request.NewStatus, message = "状态更新成功，已生成流转日志。" });
        }

        //删除TNR案例
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTnrCase(string id)
        {
            var existing = await _tnrCaseRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的TNR案例，无法删除。");

            await _tnrCaseRepository.Delete(id);
            return NoContent();
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
