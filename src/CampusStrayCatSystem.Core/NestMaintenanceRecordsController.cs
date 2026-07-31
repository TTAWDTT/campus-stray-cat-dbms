using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core
{
    [Route("api/nest-maintenance-records")]
    [ApiController]
    public class NestMaintenanceRecordsController : ControllerBase
    {
        private readonly INestMaintenanceRecordRepository _maintenanceRepository;
        private readonly IServicePointRepository _pointRepository;

        public NestMaintenanceRecordsController(
            INestMaintenanceRecordRepository maintenanceRepository,
            IServicePointRepository pointRepository)
        {
            _maintenanceRepository = maintenanceRepository;
            _pointRepository = pointRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NestMaintenanceRecord>>> GetRecords(
            [FromQuery] string? pointId,
            [FromQuery] string? damageLevel,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                return BadRequest("开始时间不能晚于结束时间。");
            }

            return Ok(await _maintenanceRepository.GetAllAsync(pointId, damageLevel, from, to));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NestMaintenanceRecord>> GetRecord(string id)
        {
            var record = await _maintenanceRepository.GetByIdAsync(id);
            return record == null ? NotFound($"未找到 ID 为 {id} 的维护记录。") : Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<NestMaintenanceRecord>> CreateRecord(
            [FromBody] NestMaintenanceRecord record)
        {
            record.CheckTime ??= DateTime.UtcNow;
            var validationError = await ValidateRecordAsync(record);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            record.MaintenanceID = Guid.NewGuid().ToString();
            Normalize(record);
            await _maintenanceRepository.CreateAsync(record);

            return CreatedAtAction(nameof(GetRecord), new { id = record.MaintenanceID }, record);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRecord(
            string id,
            [FromBody] NestMaintenanceRecord record)
        {
            if (!string.IsNullOrWhiteSpace(record.MaintenanceID)
                && !string.Equals(id, record.MaintenanceID, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("URL 中的维护记录 ID 与请求体中的 ID 不匹配。");
            }

            if (await _maintenanceRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"未找到 ID 为 {id} 的维护记录。");
            }

            record.MaintenanceID = id;
            var validationError = await ValidateRecordAsync(record);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            Normalize(record);
            await _maintenanceRepository.UpdateAsync(record);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecord(string id)
        {
            if (await _maintenanceRepository.DeleteAsync(id) == 0)
            {
                return NotFound($"未找到 ID 为 {id} 的维护记录。");
            }

            return NoContent();
        }

        private async Task<string?> ValidateRecordAsync(NestMaintenanceRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.PointID))
            {
                return "猫窝点位 ID 不能为空。";
            }

            if (await _pointRepository.GetByIdAsync(record.PointID.Trim()) == null)
            {
                return $"关联点位 {record.PointID.Trim()} 不存在。";
            }

            if (string.IsNullOrWhiteSpace(record.ActionType))
            {
                return "维护动作不能为空。";
            }

            if (!string.IsNullOrWhiteSpace(record.OperatorUserID)
                && await _maintenanceRepository.UserExistsAsync(record.OperatorUserID.Trim()) == false)
            {
                return $"操作用户 {record.OperatorUserID.Trim()} 不存在。";
            }

            if (record.CheckTime.HasValue
                && record.NextCheckTime.HasValue
                && record.NextCheckTime.Value < record.CheckTime.Value)
            {
                return "下次巡查时间不能早于本次巡查时间。";
            }

            return null;
        }

        private static void Normalize(NestMaintenanceRecord record)
        {
            record.PointID = NormalizeOptional(record.PointID);
            record.MaterialType = NormalizeOptional(record.MaterialType);
            record.WeatherCondition = NormalizeOptional(record.WeatherCondition);
            record.DamageLevel = NormalizeOptional(record.DamageLevel);
            record.ActionType = NormalizeOptional(record.ActionType);
            record.OperatorUserID = NormalizeOptional(record.OperatorUserID);
            record.Remark = NormalizeOptional(record.Remark);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
