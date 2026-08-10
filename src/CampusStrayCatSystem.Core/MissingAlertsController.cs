using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
    [Authorize]
    public class MissingAlertsController : ControllerBase
    {
        private readonly IMissingAlertRepository _missingAlertRepository;
        private readonly ICatRepository _catRepository;
        private readonly ICampusAreaRepository _areaRepository;
        private readonly ICatSightingRepository _sightingRepository;
        private readonly IUserRepository _userRepository;

        public MissingAlertsController(
            IMissingAlertRepository missingAlertRepository,
            ICatRepository catRepository,
            ICampusAreaRepository areaRepository,
            ICatSightingRepository sightingRepository,
            IUserRepository userRepository)
        {
            _missingAlertRepository = missingAlertRepository;
            _catRepository = catRepository;
            _areaRepository = areaRepository;
            _sightingRepository = sightingRepository;
            _userRepository = userRepository;
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
            if (string.IsNullOrWhiteSpace(catId))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            if (!await _catRepository.Exists(catId.Trim()))
            {
                return NotFound($"未找到猫咪 {catId}。");
            }

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
            if (string.IsNullOrWhiteSpace(alertId))
            {
                return BadRequest("预警 ID 不能为空。");
            }

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

            if (string.IsNullOrWhiteSpace(sighting.CatID))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            sighting.CatID = sighting.CatID.Trim();
            if (!await _catRepository.Exists(sighting.CatID))
            {
                return NotFound($"未找到猫咪 {sighting.CatID}。");
            }

            if (string.IsNullOrWhiteSpace(sighting.AreaID))
            {
                return BadRequest("区域 ID 不能为空。");
            }

            sighting.AreaID = sighting.AreaID.Trim();
            if (await _areaRepository.GetByIdAsync(sighting.AreaID) == null)
            {
                return NotFound($"未找到区域 {sighting.AreaID}。");
            }

            if (sighting.SightingTime == null)
            {
                return BadRequest("目击时间不能为空。");
            }

            sighting.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sighting.UserID)) return Unauthorized();

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

            if (string.IsNullOrWhiteSpace(alert.CatID))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            alert.CatID = alert.CatID.Trim();
            if (!await _catRepository.Exists(alert.CatID))
            {
                return NotFound($"未找到猫咪 {alert.CatID}。");
            }

            if (!string.IsNullOrWhiteSpace(alert.LastSightingID))
            {
                alert.LastSightingID = alert.LastSightingID.Trim();
                var sighting = await _sightingRepository.GetByIdAsync(alert.LastSightingID);
                if (sighting == null)
                {
                    return NotFound($"未找到最后目击记录 {alert.LastSightingID}。");
                }

                if (!string.Equals(sighting.CatID, alert.CatID, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("最后目击记录不属于该猫咪。");
                }
            }

            var existingAlerts = await _missingAlertRepository.GetByCatId(alert.CatID);
            if (existingAlerts.Any(existing =>
                    string.Equals(existing.AlertStatus, MissingAlertStatuses.Processing, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict("该猫已有处理中预警。");
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

            try
            {
                await _missingAlertRepository.CreateAlert(alert);
            }
            catch (Exception ex) when (ContainsOracleError(ex, "ORA-20163"))
            {
                return Conflict("该猫已有处理中预警。");
            }
            catch (Exception ex) when (ContainsOracleError(ex, "ORA-20162"))
            {
                return NotFound("最后目击记录不存在或不属于该猫咪。");
            }

            return CreatedAtAction(nameof(GetById), new { alertId = alert.AlertID }, alert);
        }

        /// <summary>
        /// 更新预警状态。
        /// 支持处理中、已寻回、已关闭等状态流转。
        /// </summary>
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

            if (string.IsNullOrWhiteSpace(alert.HandlerUserID))
            {
                return BadRequest("处理人 ID 不能为空。");
            }

            var handlerUserId = alert.HandlerUserID.Trim();
            var handler = await _userRepository.GetById(handlerUserId);
            if (handler == null || !UserStatusCodes.IsActive(handler.Status)
                || !(string.Equals(handler.RoleName, "ADMIN", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(handler.RoleName, "VOLUNTEER", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("处理人必须是有效的管理员或志愿者。");
            }

            if (await _missingAlertRepository.GetById(alertId) == null)
            {
                return NotFound($"未找到预警 {alertId}。");
            }

            var rows = await _missingAlertRepository.UpdateStatus(
                alertId,
                alert.AlertStatus.Trim().ToUpperInvariant(),
                handlerUserId,
                alert.Remark);
            return NoContent();
        }

        private static bool ContainsOracleError(Exception ex, string code)
        {
            return ex.ToString().Contains(code, StringComparison.OrdinalIgnoreCase);
        }
    }
}
