using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core {
    [Route("api/areas")]
    [ApiController]
    public class AreasController : ControllerBase {
        private readonly ICampusAreaRepository _campusAreaRepository;

        public AreasController(ICampusAreaRepository campusAreaRepository) { _campusAreaRepository = campusAreaRepository; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampusArea>>> GetAreas() {
            var areas = await _campusAreaRepository.GetAllAsync();
            return Ok(areas);}

        [HttpGet("{areaId}")] public async Task<ActionResult<CampusArea>> GetArea(string areaId) {
            var area = await _campusAreaRepository.GetByIdAsync(areaId);
            if (area == null) { return NotFound(new { message = $"未找到 ID 为 {areaId} 的校园区域。" });}

            return Ok(area);}
    }
}
