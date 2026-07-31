using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core {
    [Route("api/cats")]
    [ApiController] public class CatsController : ControllerBase {
        private readonly ICatRepository _catRepository;

        public CatsController(ICatRepository catRepository) { _catRepository = catRepository; }

        [HttpGet] public async Task<ActionResult<IEnumerable<CatSummary>>> GetCats([FromQuery] string? mainAreaId = null,
                                                                                  [FromQuery] string? lifeStatus = null,
                                                                                  [FromQuery] string? archiveStatus = null) {
            var cats = await _catRepository.GetAllAsync(mainAreaId, lifeStatus, archiveStatus);
            return Ok(cats);}

        [HttpGet("{catId}")]
        public async Task<ActionResult<CatSummary>> GetCat(string catId) {
            var cat = await _catRepository.GetByIdAsync(catId);
            if (cat == null) { return NotFound(new { message = $"未找到 ID 为 {catId} 的猫咪档案。" });}

            return Ok(cat);}
    }
}
