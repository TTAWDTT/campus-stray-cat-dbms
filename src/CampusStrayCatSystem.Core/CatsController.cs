using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core {
    [Route("api/cats")]
    [ApiController] public class CatsController : ControllerBase {
        private readonly ICatRepository _catRepository;
        private readonly ICampusAreaRepository _campusAreaRepository;

        public CatsController(ICatRepository catRepository, ICampusAreaRepository campusAreaRepository) {
            _catRepository = catRepository;
            _campusAreaRepository = campusAreaRepository;}

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

        [ProducesResponseType<CatSummary>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost] public async Task<ActionResult<CatSummary>> CreateCat(CreateCatRequest request) {
            if (request.MainAreaId != null &&
                await _campusAreaRepository.GetByIdAsync(request.MainAreaId) == null) { return BadRequest(new { message = $"未找到 ID 为 {request.MainAreaId} 的校园区域。" });}

            var cat = new Cat {
                CatId = Guid.NewGuid().ToString(),
                CatName = request.CatName,
                Gender = request.Gender,
                Breed = request.Breed,
                ColorPattern = request.ColorPattern,
                SterilizedFlag = request.SterilizedFlag,
                EarTipFlag = request.EarTipFlag,
                PersonalityTags = request.PersonalityTags,
                MainAreaId = request.MainAreaId,
                LifeStatus = request.LifeStatus,
                ArchiveStatus = CatStatusCodes.ArchiveDraft};

            var createdCat = await _catRepository.CreateAsync(cat);
            if (createdCat == null) { return StatusCode(500, new { message = "猫咪档案创建失败，数据库操作已回滚。" });}

            return CreatedAtAction(nameof(GetCat), new { catId = cat.CatId }, createdCat);}
    }
}
