using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CampusStrayCatSystem.Core
{
    [Route("api/cat-sightings")]
    [ApiController]
    public class CatSightingsController : ControllerBase
    {
        private readonly ICatSightingRepository _sightingRepository;
        private readonly ICampusAreaRepository _areaRepository;

        public CatSightingsController(
            ICatSightingRepository sightingRepository,
            ICampusAreaRepository areaRepository)
        {
            _sightingRepository = sightingRepository;
            _areaRepository = areaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatSighting>>> GetSightings(
            [FromQuery] string? catId,
            [FromQuery] string? areaId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                return BadRequest("开始时间不能晚于结束时间。");
            }

            return Ok(await _sightingRepository.GetAllAsync(catId, areaId, from, to));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CatSighting>> GetSighting(string id)
        {
            var sighting = await _sightingRepository.GetByIdAsync(id);
            return sighting == null ? NotFound($"未找到 ID 为 {id} 的目击记录。") : Ok(sighting);
        }

        [HttpGet("recent/by-cat/{catId}")]
        public async Task<ActionResult<IEnumerable<CatSighting>>> GetRecentByCat(
            string catId,
            [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(catId))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            if (limit is < 1 or > 100)
            {
                return BadRequest("limit 必须在 1 到 100 之间。");
            }

            return Ok(await _sightingRepository.GetRecentByCatAsync(catId, limit));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CatSighting>> CreateSighting([FromBody] CatSighting sighting)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }
            sighting.UserID = userId;
            sighting.SightingTime ??= DateTime.UtcNow;
            var validationError = await ValidateSightingAsync(sighting);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            sighting.SightingID = Guid.NewGuid().ToString();
            Normalize(sighting);
            await _sightingRepository.CreateAsync(sighting);

            return CreatedAtAction(nameof(GetSighting), new { id = sighting.SightingID }, sighting);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> UpdateSighting(string id, [FromBody] CatSighting sighting)
        {
            if (sighting == null)
            {
                return BadRequest("目击记录数据不能为空。");
            }

            if (!string.IsNullOrWhiteSpace(sighting.SightingID)
                && !string.Equals(id, sighting.SightingID, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("URL 中的目击记录 ID 与请求体中的 ID 不匹配。");
            }

            var existing = await _sightingRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound($"未找到 ID 为 {id} 的目击记录。");
            }

            sighting.SightingID = id;
            sighting.UserID = existing.UserID;
            var validationError = await ValidateSightingAsync(sighting);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            Normalize(sighting);
            await _sightingRepository.UpdateAsync(sighting);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> DeleteSighting(string id)
        {
            if (await _sightingRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"未找到 ID 为 {id} 的目击记录。");
            }

            if (await _sightingRepository.HasReferencesAsync(id))
            {
                return Conflict("该目击记录已被失踪预警引用，不能删除。");
            }

            await _sightingRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<string?> ValidateSightingAsync(CatSighting sighting)
        {
            if (string.IsNullOrWhiteSpace(sighting.CatID))
            {
                return "猫咪 ID 不能为空。";
            }

            if (string.IsNullOrWhiteSpace(sighting.UserID))
            {
                return "上报用户 ID 不能为空。";
            }

            if (string.IsNullOrWhiteSpace(sighting.AreaID))
            {
                return "目击区域 ID 不能为空。";
            }

            if (await _sightingRepository.CatExistsAsync(sighting.CatID.Trim()) == false)
            {
                return $"关联猫咪 {sighting.CatID.Trim()} 不存在。";
            }

            if (await _sightingRepository.UserExistsAsync(sighting.UserID.Trim()) == false)
            {
                return $"上报用户 {sighting.UserID.Trim()} 不存在。";
            }

            if (await _areaRepository.GetByIdAsync(sighting.AreaID.Trim()) == null)
            {
                return $"关联区域 {sighting.AreaID.Trim()} 不存在。";
            }

            return ValidateCoordinates(sighting.Longitude, sighting.Latitude);
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

        private static void Normalize(CatSighting sighting)
        {
            sighting.CatID = NormalizeOptional(sighting.CatID);
            sighting.UserID = NormalizeOptional(sighting.UserID);
            sighting.AreaID = NormalizeOptional(sighting.AreaID);
            sighting.PhotoUrl = NormalizeOptional(sighting.PhotoUrl);
            sighting.Remark = NormalizeOptional(sighting.Remark);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
