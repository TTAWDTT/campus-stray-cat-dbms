using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CampusStrayCatSystem.Core
{
    [Route("api/service-points")]
    [ApiController]
    public class ServicePointsController : ControllerBase
    {
        private readonly IServicePointRepository _pointRepository;
        private readonly ICampusAreaRepository _areaRepository;

        public ServicePointsController(
            IServicePointRepository pointRepository,
            ICampusAreaRepository areaRepository)
        {
            _pointRepository = pointRepository;
            _areaRepository = areaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicePoint>>> GetPoints(
            [FromQuery] string? areaId,
            [FromQuery] string? pointType,
            [FromQuery] string? facilityStatus)
        {
            return Ok(await _pointRepository.GetAllAsync(areaId, pointType, facilityStatus));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServicePoint>> GetPoint(string id)
        {
            var point = await _pointRepository.GetByIdAsync(id);
            return point == null ? NotFound($"未找到 ID 为 {id} 的服务点。") : Ok(point);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<ServicePoint>> CreatePoint([FromBody] ServicePoint point)
        {
            var validationError = await ValidatePointAsync(point);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            point.PointID = Guid.NewGuid().ToString();
            Normalize(point);
            await _pointRepository.CreateAsync(point);

            return CreatedAtAction(nameof(GetPoint), new { id = point.PointID }, point);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> UpdatePoint(string id, [FromBody] ServicePoint point)
        {
            if (!string.IsNullOrWhiteSpace(point.PointID)
                && !string.Equals(id, point.PointID, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("URL 中的点位 ID 与请求体中的点位 ID 不匹配。");
            }

            if (await _pointRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"未找到 ID 为 {id} 的服务点。");
            }

            point.PointID = id;
            var validationError = await ValidatePointAsync(point);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            Normalize(point);
            await _pointRepository.UpdateAsync(point);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> DeletePoint(string id)
        {
            if (await _pointRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"未找到 ID 为 {id} 的服务点。");
            }

            if (await _pointRepository.HasReferencesAsync(id))
            {
                return Conflict("该服务点仍被排班或维护记录使用，不能删除。");
            }

            await _pointRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<string?> ValidatePointAsync(ServicePoint point)
        {
            if (string.IsNullOrWhiteSpace(point.PointName))
            {
                return "点位名称不能为空。";
            }

            if (point.PointName.Trim().Length > 100)
            {
                return "点位名称不能超过 100 个字符。";
            }

            if (!string.IsNullOrWhiteSpace(point.AreaID)
                && await _areaRepository.GetByIdAsync(point.AreaID.Trim()) == null)
            {
                return $"关联区域 {point.AreaID.Trim()} 不存在。";
            }

            if (!string.IsNullOrWhiteSpace(point.PointType) && !ServicePointTypes.IsValid(point.PointType))
            {
                return $"点位类型必须是 {string.Join("、", ServicePointTypes.Allowed)}。";
            }

            if (!string.IsNullOrWhiteSpace(point.FacilityStatus) && !FacilityStatuses.IsValid(point.FacilityStatus))
            {
                return $"设施状态必须是 {string.Join("、", FacilityStatuses.Allowed)}。";
            }

            return ValidateCoordinates(point.Longitude, point.Latitude);
        }

        private static string? ValidateCoordinates(decimal? longitude, decimal? latitude)
        {
            if (longitude.HasValue != latitude.HasValue)
            {
                return "经度和纬度必须同时提供。";
            }

            if (longitude.HasValue && (longitude.Value < -180m || longitude.Value > 180m))
            {
                return "经度必须在 -180 到 180 之间。";
            }

            if (latitude.HasValue && (latitude.Value < -90m || latitude.Value > 90m))
            {
                return "纬度必须在 -90 到 90 之间。";
            }

            return null;
        }

        private static void Normalize(ServicePoint point)
        {
            point.PointName = point.PointName.Trim();
            point.AreaID = NormalizeOptional(point.AreaID);
            point.PointType = ServicePointTypes.Normalize(point.PointType);
            point.FacilityStatus = FacilityStatuses.Normalize(point.FacilityStatus);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
