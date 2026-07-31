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

        [ProducesResponseType<IEnumerable<CatSummary>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet] public async Task<ActionResult<IEnumerable<CatSummary>>> GetCats([FromQuery] string? mainAreaId = null,
                                                                                  [FromQuery] string? lifeStatus = null,
                                                                                  [FromQuery] string? archiveStatus = null) {
            mainAreaId = NormalizeOptional(mainAreaId);
            lifeStatus = CatStatusCodes.NormalizeLifeStatus(lifeStatus);
            archiveStatus = CatStatusCodes.NormalizeArchiveStatus(archiveStatus);
            if (lifeStatus != null && !IsValidLifeStatus(lifeStatus)) {
                return BadRequest(new { message = "生活状态只能是 ON_CAMPUS、MISSING、ADOPTED 或 DECEASED。" });}
            if (archiveStatus != null && !IsValidArchiveStatus(archiveStatus)) {
                return BadRequest(new { message = "档案状态只能是 DRAFT、PUBLISHED 或 ARCHIVED。" });}

            var cats = await _catRepository.GetAllAsync(mainAreaId, lifeStatus, archiveStatus);
            return Ok(cats);}

        [HttpGet("{catId}")]
        public async Task<ActionResult<CatSummary>> GetCat(string catId) {
            var cat = await _catRepository.GetByIdAsync(catId);
            if (cat == null) {
                return NotFound(new { message = $"未找到 ID 为 {catId} 的猫咪档案。" });}

            return Ok(cat);}

        [ProducesResponseType<CatSummary>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost] public async Task<ActionResult<CatSummary>> CreateCat(CreateCatRequest request) {
            if (request.MainAreaId != null &&
                await _campusAreaRepository.GetByIdAsync(request.MainAreaId) == null) {
                return BadRequest(new { message = $"未找到 ID 为 {request.MainAreaId} 的校园区域。" });}

            var cat = new Cat {
                CatID = Guid.NewGuid().ToString(),
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
            if (createdCat == null) {
                return StatusCode(500, new { message = "猫咪档案创建失败，数据库操作已回滚。" });}

            return CreatedAtAction(nameof(GetCat), new { catId = cat.CatID }, createdCat);}

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{catId}")]
        public async Task<IActionResult> UpdateCat(string catId, UpdateCatRequest request) {
            var existingCat = await _catRepository.GetByIdAsync(catId);
            if (existingCat == null) {
                return NotFound(new { message = $"未找到 ID 为 {catId} 的猫咪档案。" });}

            if (request.MainAreaId != null &&
                await _campusAreaRepository.GetByIdAsync(request.MainAreaId) == null) {
                return BadRequest(new { message = $"未找到 ID 为 {request.MainAreaId} 的校园区域。" });}

            var cat = new Cat {
                CatID = catId,
                CatName = request.CatName,
                Gender = request.Gender,
                Breed = request.Breed,
                ColorPattern = request.ColorPattern,
                SterilizedFlag = request.SterilizedFlag,
                EarTipFlag = request.EarTipFlag,
                PersonalityTags = request.PersonalityTags,
                MainAreaId = request.MainAreaId,
                LifeStatus = request.LifeStatus,
                ArchiveStatus = request.ArchiveStatus};

            var affectedRows = await _catRepository.UpdateAsync(cat);
            if (affectedRows == 0) {
                return NotFound(new { message = $"未找到 ID 为 {catId} 的猫咪档案。" });}

            return NoContent();}

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{catId}")] public async Task<IActionResult> ArchiveCat(string catId) {
            var affectedRows = await _catRepository.ArchiveAsync(catId);
            if (affectedRows == 0) {
                return NotFound(new { message = $"未找到 ID 为 {catId} 的猫咪档案。" });}

            return NoContent();}

        private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static bool IsValidLifeStatus(string value) => value is CatStatusCodes.LifeOnCampus or CatStatusCodes.LifeMissing or CatStatusCodes.LifeAdopted or CatStatusCodes.LifeDeceased;
        private static bool IsValidArchiveStatus(string value) => value is CatStatusCodes.ArchiveDraft or CatStatusCodes.ArchivePublished or CatStatusCodes.ArchiveArchived;
    }
}
