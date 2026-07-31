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
            return Ok(logs ?? new List<TnrStatusLog>());
        }
    }
}
