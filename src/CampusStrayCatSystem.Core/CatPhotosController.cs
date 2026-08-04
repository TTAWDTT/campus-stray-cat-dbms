using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CampusStrayCatSystem.Core {
    [Route("api/cats/{catId}/photos")]
    [ApiController] public class CatPhotosController : ControllerBase {
        private const long MaximumMultipartBodySize = CatPhotoFileStorage.MaximumFileSize + 64 * 1024;
        private readonly CatPhotoService _catPhotoService;

        public CatPhotosController(CatPhotoService catPhotoService) { _catPhotoService = catPhotoService; }

        [ProducesResponseType<IEnumerable<CatPhoto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet] public async Task<ActionResult<IEnumerable<CatPhoto>>> GetPhotos(string catId) {
            var result = await _catPhotoService.GetAllAsync(catId);
            if (result.Status == CatPhotoServiceStatus.InvalidIdentifier) {
                return BadRequest(new { message = "猫咪 ID 格式不正确。" });}
            if (result.Status == CatPhotoServiceStatus.CatNotFound) {
                return NotFound(new { message = $"未找到 ID 为 {catId} 的猫咪档案。" });}
            return Ok(result.Value);}

        [ProducesResponseType<CatPhoto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{photoId}")]
        public async Task<ActionResult<CatPhoto>> GetPhoto(string catId, string photoId) {
            var result = await _catPhotoService.GetByIDAsync(catId, photoId);
            if (result.Status == CatPhotoServiceStatus.InvalidIdentifier) {
                return BadRequest(new { message = "猫咪 ID 或照片 ID 格式不正确。" });}
            if (result.Status == CatPhotoServiceStatus.PhotoNotFound) {
                return NotFound(new { message = $"未找到 ID 为 {photoId} 的猫咪照片。" });}
            return Ok(result.Value);}

        [Consumes("multipart/form-data")]
        [ProducesResponseType<CatPhoto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [RequestSizeLimit(MaximumMultipartBodySize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaximumMultipartBodySize)]
        [HttpPost] [Authorize(Roles = "ADMIN,VOLUNTEER")] public async Task<ActionResult<CatPhoto>> UploadPhoto(string catId,
                                                                        [FromForm] UploadCatPhotoRequest request,
                                                                        CancellationToken cancellationToken) {
            var result = await _catPhotoService.UploadAsync(catId, request, cancellationToken);
            if (result.Status == CatPhotoServiceStatus.InvalidIdentifier) {
                return BadRequest(new { message = "猫咪 ID 格式不正确。" });}
            if (result.Status is CatPhotoServiceStatus.InvalidUploadUserID or
                                 CatPhotoServiceStatus.UploadUserNotFound) {
                return BadRequest(new { message = "上传用户 ID 为空、过长或对应用户不存在。" });}
            if (result.Status == CatPhotoServiceStatus.InvalidPrimaryFlag) {
                return BadRequest(new { message = "主图标志只能是 0 或 1。" });}
            if (result.Status == CatPhotoServiceStatus.EmptyFile) {
                return BadRequest(new { message = "照片文件不能为空。" });}
            if (result.Status == CatPhotoServiceStatus.FileTooLarge) {
                return BadRequest(new { message = "照片文件不能超过 10 MiB。" });}
            if (result.Status == CatPhotoServiceStatus.UnsupportedFormat) {
                return BadRequest(new { message = "照片只能是扩展名、MIME 类型和文件头一致的 JPEG 或 PNG。" });}
            if (result.Status == CatPhotoServiceStatus.CatNotFound) {
                return NotFound(new { message = $"未找到 ID 为 {catId} 的猫咪档案。" });}
            if (result.Status == CatPhotoServiceStatus.CatArchived) {
                return Conflict(new { message = "已归档的猫咪档案不能上传照片。" });}

            return CreatedAtAction(nameof(GetPhoto),
                                   new { catId, photoId = result.Value!.PhotoID },
                                   result.Value);}

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPut("{photoId}/primary")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> SetPrimary(string catId, string photoId) {
            var status = await _catPhotoService.SetPrimaryAsync(catId, photoId);
            if (status == CatPhotoServiceStatus.InvalidIdentifier) {
                return BadRequest(new { message = "猫咪 ID 或照片 ID 格式不正确。" });}
            if (status is CatPhotoServiceStatus.CatNotFound or CatPhotoServiceStatus.PhotoNotFound) {
                return NotFound(new { message = $"未找到 ID 为 {photoId} 的猫咪照片。" });}
            if (status == CatPhotoServiceStatus.CatArchived) {
                return Conflict(new { message = "已归档的猫咪档案不能切换主图。" });}
            return NoContent();}

        [ProducesResponseType<CatPhotoFeatureResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{photoId}/feature")] public async Task<ActionResult<CatPhotoFeatureResponse>> GetFeature(string catId,
                                                                                                          string photoId) {
            var result = await _catPhotoService.GetFeatureAsync(catId, photoId);
            if (result.Status == CatPhotoServiceStatus.InvalidIdentifier) {
                return BadRequest(new { message = "猫咪 ID 或照片 ID 格式不正确。" });}
            if (result.Status == CatPhotoServiceStatus.PhotoNotFound) {
                return NotFound(new { message = $"未找到 ID 为 {photoId} 的猫咪照片。" });}
            if (result.Status == CatPhotoServiceStatus.InvalidFeature) {
                return StatusCode(500, new { message = "数据库中的照片特征不是合法的 JSON 数值数组。" });}
            return Ok(result.Value);}

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpDelete("{photoId}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        public async Task<IActionResult> DeletePhoto(string catId, string photoId) {
            var status = await _catPhotoService.DeleteAsync(catId, photoId);
            if (status == CatPhotoServiceStatus.InvalidIdentifier) {
                return BadRequest(new { message = "猫咪 ID 或照片 ID 格式不正确。" });}
            if (status is CatPhotoServiceStatus.CatNotFound or CatPhotoServiceStatus.PhotoNotFound) {
                return NotFound(new { message = $"未找到 ID 为 {photoId} 的猫咪照片。" });}
            if (status == CatPhotoServiceStatus.PhotoReferenced) {
                return Conflict(new { message = "该照片已被匹配记录引用，不能删除。" });}
            return NoContent();}
    }
}
