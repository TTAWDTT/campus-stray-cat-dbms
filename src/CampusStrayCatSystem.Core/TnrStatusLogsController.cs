using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class TnrStatusLogsController : ControllerBase
    {
        private readonly ITnrStatusLogRepository _tnrStatusLogRepository;
        private readonly ITnrCaseRepository _tnrCaseRepository;

        public TnrStatusLogsController(
            ITnrStatusLogRepository tnrStatusLogRepository,
            ITnrCaseRepository tnrCaseRepository)
        {
            _tnrStatusLogRepository = tnrStatusLogRepository;
            _tnrCaseRepository = tnrCaseRepository;
        }

        //查看某个TNR案例的完整状态流转记录
        [HttpGet("case/{caseId}")]
        public async Task<ActionResult<IEnumerable<TnrStatusLog>>> GetLogsByCaseId(string caseId)
        {
            var tnrCase = await _tnrCaseRepository.GetById(caseId);
            if (tnrCase == null)
                return NotFound($"未找到 ID 为 {caseId} 的TNR案例。");

            var logs = await _tnrStatusLogRepository.GetByCaseId(caseId);

            if (logs == null || !logs.Any())
                return Ok(new List<TnrStatusLog>()); // 返回空数组

            return Ok(logs);
        }

        //手动新增一条状态日志（用于补录等场景）
        [HttpPost]
        public async Task<ActionResult<TnrStatusLog>> CreateLog([FromBody] TnrStatusLog log)
        {
            if (log == null)
                return BadRequest("日志数据为空，无法创建。");

            if (string.IsNullOrWhiteSpace(log.CaseID))
                return BadRequest("案例编号不能为空。");

            var tnrCase = await _tnrCaseRepository.GetById(log.CaseID);
            if (tnrCase == null)
                return NotFound($"未找到 ID 为 {log.CaseID} 的TNR案例。");

            await _tnrStatusLogRepository.Create(log);
            return CreatedAtAction(nameof(GetLogsByCaseId), new { caseId = log.CaseID }, log);
        }
    }
}
