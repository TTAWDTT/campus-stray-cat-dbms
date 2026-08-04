using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CampusStrayCatSystem.Core
{
    [Route("api/campus-areas")]
    [ApiController]
    public class CampusAreasController : ControllerBase
    {
        private readonly ICampusAreaRepository _areaRepository;

        public CampusAreasController(ICampusAreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampusArea>>> GetAreas(
            [FromQuery] string? campusName,
            [FromQuery] string? areaType,
            [FromQuery] string? riskLevel)
        {
            var areas = await _areaRepository.GetAllAsync(campusName, areaType, riskLevel);
            return Ok(areas);
        }

        [HttpGet("roots")]
        public async Task<ActionResult<IEnumerable<CampusArea>>> GetRootAreas()
        {
            return Ok(await _areaRepository.GetRootsAsync());
        }

        [HttpGet("hierarchy")]
        public async Task<ActionResult<IEnumerable<CampusAreaHierarchyItem>>> GetHierarchy()
        {
            return Ok(await _areaRepository.GetHierarchyAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CampusArea>> GetArea(string id)
        {
            var area = await _areaRepository.GetByIdAsync(id);
            return area == null ? NotFound($"未找到 ID 为 {id} 的校园区域。") : Ok(area);
        }

        [HttpGet("{id}/children")]
        public async Task<ActionResult<IEnumerable<CampusArea>>> GetChildren(string id)
        {
            if (await _areaRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"未找到 ID 为 {id} 的校园区域。");
            }

            return Ok(await _areaRepository.GetChildrenAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<ActionResult<CampusArea>> CreateArea([FromBody] CampusArea area)
        {
            var validationError = await ValidateAreaAsync(area, null);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            area.AreaID = Guid.NewGuid().ToString();
            Normalize(area);
            await _areaRepository.CreateAsync(area);

            return CreatedAtAction(nameof(GetArea), new { id = area.AreaID }, area);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> UpdateArea(string id, [FromBody] CampusArea area)
        {
            if (!string.IsNullOrWhiteSpace(area.AreaID)
                && !string.Equals(id, area.AreaID, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("URL 中的区域 ID 与请求体中的区域 ID 不匹配。");
            }

            if (await _areaRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"未找到 ID 为 {id} 的校园区域。");
            }

            area.AreaID = id;
            var validationError = await ValidateAreaAsync(area, id);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            Normalize(area);
            await _areaRepository.UpdateAsync(area);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> DeleteArea(string id)
        {
            if (await _areaRepository.GetByIdAsync(id) == null)
            {
                return NotFound($"未找到 ID 为 {id} 的校园区域。");
            }

            if ((await _areaRepository.GetChildrenAsync(id)).Any())
            {
                return Conflict("该区域仍包含下级区域，不能直接删除。");
            }

            if (await _areaRepository.HasReferencesAsync(id))
            {
                return Conflict("该区域仍被猫咪、服务点、目击或紧急上报记录使用，不能删除。");
            }

            await _areaRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<string?> ValidateAreaAsync(CampusArea area, string? currentAreaId)
        {
            if (string.IsNullOrWhiteSpace(area.AreaName))
            {
                return "区域名称不能为空。";
            }

            if (area.AreaName.Trim().Length > 100)
            {
                return "区域名称不能超过 100 个字符。";
            }

            if (string.IsNullOrWhiteSpace(area.ParentAreaID))
            {
                return null;
            }

            var parentId = area.ParentAreaID.Trim();
            if (string.Equals(currentAreaId, parentId, StringComparison.OrdinalIgnoreCase))
            {
                return "区域不能将自身设置为父级区域。";
            }

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currentId = parentId;
            while (!string.IsNullOrWhiteSpace(currentId))
            {
                if (!visited.Add(currentId))
                {
                    return "父级区域关系中存在循环。";
                }

                if (string.Equals(currentAreaId, currentId, StringComparison.OrdinalIgnoreCase))
                {
                    return "该父级区域会形成循环层级。";
                }

                var parent = await _areaRepository.GetByIdAsync(currentId);
                if (parent == null)
                {
                    return $"父级区域 {currentId} 不存在。";
                }

                currentId = parent.ParentAreaID;
            }

            return null;
        }

        private static void Normalize(CampusArea area)
        {
            area.AreaName = area.AreaName.Trim();
            area.CampusName = NormalizeOptional(area.CampusName);
            area.ParentAreaID = NormalizeOptional(area.ParentAreaID);
            area.AreaType = NormalizeOptional(area.AreaType);
            area.RiskLevel = NormalizeOptional(area.RiskLevel);
            area.GeoBoundary = NormalizeOptional(area.GeoBoundary);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
