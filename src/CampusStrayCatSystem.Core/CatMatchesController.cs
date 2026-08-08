using System.Security.Claims;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core {
    [Route("api")]
    [ApiController]
    [Authorize] public class CatMatchesController : ControllerBase {
        private readonly ICatMatchRepository _matchRepository;
        private readonly ICatPhotoRepository _photoRepository;

        public CatMatchesController(ICatMatchRepository matchRepository,
                                     ICatPhotoRepository photoRepository) {
            _matchRepository = matchRepository;
            _photoRepository = photoRepository;
        }

        [ProducesResponseType<IEnumerable<CatMatchRecord>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("cats/{catId}/photos/{photoId}/matches")] public async Task<ActionResult<IEnumerable<CatMatchRecord>>> GetMatches(
            string catId,
            string photoId,
            [FromQuery] string? candidateCatId = null,
            [FromQuery] string? confirmStatus = null) {
            if (!CatPhotoValidation.IsSafePathIdentifier(catId) ||
                !CatPhotoValidation.IsSafePathIdentifier(photoId)) {
                return BadRequest(new { message = "猫咪 ID 或照片 ID 格式不正确。" });}
            if (!string.IsNullOrWhiteSpace(candidateCatId) &&
                !CatPhotoValidation.IsSafePathIdentifier(candidateCatId)) {
                return BadRequest(new { message = "候选猫 ID 格式不正确。" });}
            if (!string.IsNullOrWhiteSpace(confirmStatus) && !CatMatchStatusCodes.IsKnown(confirmStatus)) {
                return BadRequest(new { message = "确认状态只能是 PENDING、CONFIRMED 或 REJECTED。" });}

            var photo = await _photoRepository.GetByIDAsync(catId, photoId);
            if (photo == null) {
                return NotFound(new { message = $"未找到 ID 为 {photoId} 的猫咪照片。" });}

            var records = await _matchRepository.GetBySourcePhotoAsync(catId,
                                                                       photoId,
                                                                       NormalizeFilter(candidateCatId),
                                                                       NormalizeStatusFilter(confirmStatus));
            return Ok(records);}

        [ProducesResponseType<CatMatchRecord>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("cat-matches/{matchId}")]
        public async Task<ActionResult<CatMatchRecord>> GetMatch(string matchId) {
            if (!CatPhotoValidation.IsSafePathIdentifier(matchId)) {
                return BadRequest(new { message = "匹配记录 ID 格式不正确。" });}

            var record = await _matchRepository.GetByIDAsync(matchId);
            if (record == null) {
                return NotFound(new { message = $"未找到 ID 为 {matchId} 的匹配记录。" });}

            return Ok(record);}

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPatch("cat-matches/{matchId}/confirmation")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")] public async Task<IActionResult> ConfirmMatch(
            string matchId,
            [FromBody] ConfirmCatMatchRequest request) {
            if (!CatPhotoValidation.IsSafePathIdentifier(matchId)) {
                return BadRequest(new { message = "匹配记录 ID 格式不正确。" });}
            if (request == null || string.IsNullOrWhiteSpace(request.ConfirmStatus)) {
                return BadRequest(new { message = "确认状态不能为空。" });}

            var normalizedStatus = CatMatchStatusCodes.Normalize(request.ConfirmStatus);
            if (!CatMatchStatusCodes.IsDecision(normalizedStatus)) {
                return BadRequest(new { message = "确认状态只能是 CONFIRMED 或 REJECTED。" });}

            var confirmUserID = CurrentUserID();
            if (string.IsNullOrWhiteSpace(confirmUserID)) {
                return Unauthorized(new { message = "登录状态无效，请重新登录。" });}

            var status = await _matchRepository.ConfirmAsync(matchId, normalizedStatus, confirmUserID);
            if (status == CatMatchMutationStatus.MatchNotFound) {
                return NotFound(new { message = $"未找到 ID 为 {matchId} 的匹配记录。" });}
            if (status == CatMatchMutationStatus.AssociationUnavailable) {
                return Conflict(new { message = "匹配记录的来源照片或候选猫关联已失效，不能确认。" });}

            return NoContent();}

        private string? CurrentUserID() => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        private static string? NormalizeFilter(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeStatusFilter(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : CatMatchStatusCodes.Normalize(value);
    }
}
