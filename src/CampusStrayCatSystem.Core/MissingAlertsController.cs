using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Core
{
    /// <summary>
    /// 猫咪失踪预警接口。
    /// 负责记录最后目击、创建预警、更新预警状态和关闭结果。
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MissingAlertsController : ControllerBase
    {
        private readonly IMissingAlertRepository _missingAlertRepository;

        public MissingAlertsController(IMissingAlertRepository missingAlertRepository)
        {
            _missingAlertRepository = missingAlertRepository;
        }

        /// <summary>
        /// 获取全部失踪预警。
        /// 适合管理员在工作台里查看全部处理情况。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatMissingAlert>>> GetAll()
        {
            var alerts = await _missingAlertRepository.GetAll();
            return Ok(alerts);
        }

        /// <summary>
        /// 按猫咪查询预警历史。
        /// 这样可以快速看到某只猫是否出现过失踪预警。
        /// </summary>
        [HttpGet("cat/{catId}")]
        public async Task<ActionResult<IEnumerable<CatMissingAlert>>> GetByCatId(string catId)
        {
            var alerts = await _missingAlertRepository.GetByCatId(catId);
            return Ok(alerts);
        }

        /// <summary>
        /// 查看单条预警。
        /// 用于查看完整流转信息和最近处理结果。
        /// </summary>
        [HttpGet("{alertId}")]
        public async Task<ActionResult<CatMissingAlert>> GetById(string alertId)
        {
            var alert = await _missingAlertRepository.GetById(alertId);
            return alert == null ? NotFound($"未找到预警 {alertId}。") : Ok(alert);
        }

        /// <summary>
        /// 先记录一次猫咪目击信息。
        /// 预警流程里“最后目击”最好单独存，这样后续能精确追踪位置和时间。
        /// </summary>
        [HttpPost("sightings")]
        public async Task<ActionResult<CatSighting>> CreateSighting([FromBody] CatSighting sighting)
        {
            if (sighting == null)
            {
                return BadRequest("目击记录不能为空。");
            }

            await _missingAlertRepository.CreateSighting(sighting);
            return Ok(sighting);
        }

        /// <summary>
        /// 创建失踪预警。
        /// 这里会把猫、最后目击、阈值和当前处理人一起保存。
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CatMissingAlert>> Create([FromBody] CatMissingAlert alert)
        {
            if (alert == null)
            {
                return BadRequest("预警数据不能为空。");
            }

            await _missingAlertRepository.CreateAlert(alert);
            return CreatedAtAction(nameof(GetById), new { alertId = alert.AlertID }, alert);
        }

        /// <summary>
        /// 更新预警状态。
        /// 支持处理中、已寻回、已关闭等状态流转。
        /// </summary>
        [HttpPut("{alertId}/status")]
        public async Task<IActionResult> UpdateStatus(string alertId, [FromBody] CatMissingAlert alert)
        {
            if (alert == null)
            {
                return BadRequest("状态更新数据不能为空。");
            }

            var rows = await _missingAlertRepository.UpdateStatus(alertId, alert.AlertStatus, alert.HandlerUserID, alert.Remark);
            return rows == 0 ? NotFound($"未找到预警 {alertId}。") : NoContent();
        }
    }
}