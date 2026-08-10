using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Core
{
    // 猫咪失踪预警接口
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MissingAlertsController : ControllerBase
    {
        private readonly IMissingAlertRepository _missingAlertRepository;
        private readonly ICatRepository _catRepository;
        private readonly ICampusAreaRepository _areaRepository;
        private readonly ICatSightingRepository _sightingRepository;

        public MissingAlertsController(
            IMissingAlertRepository missingAlertRepository,
            ICatRepository catRepository,
            ICampusAreaRepository areaRepository,
            ICatSightingRepository sightingRepository)
        {
            _missingAlertRepository = missingAlertRepository;
            _catRepository = catRepository;
            _areaRepository = areaRepository;
            _sightingRepository = sightingRepository;
        }

        // 获取全部失踪预警
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatMissingAlert>>> GetAll()
        {
            var alerts = await _missingAlertRepository.GetAll();
            return Ok(alerts);
        }

        // 按猫咪查询预警历史
        [HttpGet("cat/{catId}")]
        public async Task<ActionResult<IEnumerable<CatMissingAlert>>> GetByCatId(string catId)
        {
            if (string.IsNullOrWhiteSpace(catId))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            var alerts = await _missingAlertRepository.GetByCatId(catId);
            return Ok(alerts);
        }

        // 查看单条预警
        [HttpGet("{alertId}")]
        public async Task<ActionResult<CatMissingAlert>> GetById(string alertId)
        {
            if (string.IsNullOrWhiteSpace(alertId))
            {
                return BadRequest("预警 ID 不能为空。");
            }

            var alert = await _missingAlertRepository.GetById(alertId);
            return alert == null ? NotFound($"未找到预警 {alertId}。") : Ok(alert);
        }

        // 记录猫咪目击信息
        [HttpPost("sightings")]
        public async Task<ActionResult<CatSighting>> CreateSighting([FromBody] CatSighting sighting)
        {
            if (sighting == null)
            {
                return BadRequest("目击记录不能为空。");
            }

            if (string.IsNullOrWhiteSpace(sighting.CatID))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            if (!await _catRepository.Exists(sighting.CatID))
                return NotFound($"未找到 ID 为 {sighting.CatID} 的猫咪档案。");

            if (string.IsNullOrWhiteSpace(sighting.AreaID))
            {
                return BadRequest("区域 ID 不能为空。");
            }

            if (await _areaRepository.GetByIdAsync(sighting.AreaID) == null)
                return NotFound($"未找到 ID 为 {sighting.AreaID} 的区域。");

            if (sighting.SightingTime == null)
            {
                return BadRequest("目击时间不能为空。");
            }

            sighting.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sighting.UserID)) return Unauthorized();

            await _missingAlertRepository.CreateSighting(sighting);
            return Ok(sighting);
        }

        // 创建失踪预警
        [HttpPost]
        public async Task<ActionResult<CatMissingAlert>> Create([FromBody] CatMissingAlert alert)
        {
            if (alert == null)
            {
                return BadRequest("预警数据不能为空。");
            }

            if (string.IsNullOrWhiteSpace(alert.CatID))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            if (!await _catRepository.Exists(alert.CatID))
                return NotFound($"未找到 ID 为 {alert.CatID} 的猫咪档案。");

            if (!string.IsNullOrWhiteSpace(alert.LastSightingID))
            {
                if (await _sightingRepository.GetByIdAsync(alert.LastSightingID) == null)
                    return NotFound($"未找到 ID 为 {alert.LastSightingID} 的最后目击记录。");
            }

            if (alert.ThresholdDays != null && alert.ThresholdDays <= 0)
            {
                return BadRequest("阈值天数必须大于 0。");
            }

            alert.HandlerUserID = null;

            alert.AlertStatus = string.IsNullOrWhiteSpace(alert.AlertStatus)
                ? MissingAlertStatuses.Processing
                : alert.AlertStatus.Trim().ToUpperInvariant();
            if (!MissingAlertStatuses.IsValid(alert.AlertStatus))
            {
                return BadRequest("预警状态必须是 PROCESSING、FOUND 或 CLOSED。");
            }

            await _missingAlertRepository.CreateAlert(alert);
            return CreatedAtAction(nameof(GetById), new { alertId = alert.AlertID }, alert);
        }

        // 更新预警状态
        [HttpPut("{alertId}/status")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> UpdateStatus(string alertId, [FromBody] CatMissingAlert alert)
        {
            if (alert == null)
            {
                return BadRequest("状态更新数据不能为空。");
            }

            if (string.IsNullOrWhiteSpace(alertId))
            {
                return BadRequest("预警 ID 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(alert.AlertStatus) ||
                !MissingAlertStatuses.IsValid(alert.AlertStatus))
            {
                return BadRequest("预警状态必须是 PROCESSING、FOUND 或 CLOSED。");
            }

            var handlerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(handlerUserId)) return Unauthorized();

            if (await _missingAlertRepository.GetById(alertId) == null)
            {
                return NotFound($"未找到预警 {alertId}。");
            }

            var rows = await _missingAlertRepository.UpdateStatus(alertId, alert.AlertStatus, handlerUserId, alert.Remark);
            return NoContent();
        }
    }
}
