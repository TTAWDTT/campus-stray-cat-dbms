using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Models.DTOs;

namespace CampusStrayCatSystem.Core.Controllers
{
    /// <summary>
    /// 用户黑名单管理控制器
    /// </summary>
    [Route("api/blacklist")]
    [ApiController]
    public class UserBlacklistController : ControllerBase
    {
        private readonly IUserBlacklistRepository _blacklistRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserBlacklistController> _logger;

        public UserBlacklistController(
            IUserBlacklistRepository blacklistRepository,
            IUserRepository userRepository,
            ILogger<UserBlacklistController> logger)
        {
            _blacklistRepository = blacklistRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取黑名单列表（仅管理员）
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(PagedResult<BlacklistResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string userId = null,
            [FromQuery] string status = null,
            [FromQuery] string keyword = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var denied = await EnsureAdminAccessAsync();
                if (denied != null) return denied;

                // 参数校验
                page = Math.Clamp(page, 1, 1_000_000);
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // 获取数据
                var items = await _blacklistRepository.GetAllAsync(userId, status, keyword, page, pageSize);
                var totalCount = await _blacklistRepository.GetTotalCountAsync(userId, status, keyword);

                var result = new PagedResult<BlacklistResponseDto>
                {
                    Items = items.Select(item => new BlacklistResponseDto
                    {
                        BlacklistId = item.BlacklistID,
                        UserId = item.UserID,
                        UserName = item.UserName,
                        ReasonType = item.ReasonType,
                        ReasonDetail = item.ReasonDetail,
                        ApplicationId = item.ApplicationID,
                        CreatedBy = item.CreateUserID,
                        CreatedByName = item.CreatedByName,
                        CreatedAt = item.CreateTime,
                        Status = item.BlacklistStatus,
                        ReleaseTime = item.ReleaseTime,
                        ReleasedBy = item.ReleasedBy,
                        ReleasedByName = item.ReleasedByName
                    }).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取黑名单列表失败");
                return StatusCode(500, new { message = "获取黑名单列表失败" });
            }
        }

        /// <summary>
        /// 获取黑名单详情（仅管理员）
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(BlacklistResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var denied = await EnsureAdminAccessAsync();
                if (denied != null) return denied;

                var record = await _blacklistRepository.GetByIdAsync(id);
                if (record == null)
                {
                    return NotFound(new { message = "黑名单记录不存在" });
                }

                return Ok(new BlacklistResponseDto
                {
                    BlacklistId = record.BlacklistID,
                    UserId = record.UserID,
                    UserName = record.UserName,
                    ReasonType = record.ReasonType,
                    ReasonDetail = record.ReasonDetail,
                    ApplicationId = record.ApplicationID,
                    CreatedBy = record.CreateUserID,
                    CreatedByName = record.CreatedByName,
                    CreatedAt = record.CreateTime,
                    Status = record.BlacklistStatus,
                    ReleaseTime = record.ReleaseTime,
                    ReleasedBy = record.ReleasedBy,
                    ReleasedByName = record.ReleasedByName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取黑名单详情失败");
                return StatusCode(500, new { message = "获取黑名单详情失败" });
            }
        }

        /// <summary>
        /// 加入黑名单（仅管理员）
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Add([FromBody] AddBlacklistDto dto)
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            // 参数校验
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 检查用户是否存在
                var user = await _userRepository.GetById(dto.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                var applicationId = string.IsNullOrWhiteSpace(dto.ApplicationId) ? null : dto.ApplicationId.Trim();
                if (applicationId != null && !await _blacklistRepository.ApplicationExistsAsync(applicationId)) {
                    return NotFound(new { message = "关联的领养申请不存在" });
                }

                // 检查用户是否已在黑名单中
                var isBlacklisted = await _blacklistRepository.HasActiveBlacklistAsync(dto.UserId);
                if (isBlacklisted)
                {
                    return Conflict(new { message = "该用户已在黑名单中，请勿重复拉黑" });
                }

                // 获取当前操作人ID（从JWT中获取）
                var operatorId = CurrentOperatorId();
                if (operatorId == null) return Unauthorized(new { message = "登录状态无效，请重新登录。" });

                var newRecord = new UserBlacklist{
                    BlacklistID = Guid.NewGuid().ToString(),
                    UserID = dto.UserId,
                    ReasonType = dto.ReasonType,
                    ReasonDetail = dto.ReasonDetail,
                    ApplicationID = applicationId,
                    CreateUserID = operatorId,
                    CreateTime = DateTime.Now,
                    BlacklistStatus = "ACTIVE"
                };

                if (!await _blacklistRepository.AddAsync(newRecord)) {
                    return Conflict(new { message = "该用户已在黑名单中，请勿重复拉黑" });
                }

                return CreatedAtAction(nameof(GetById), new { id = newRecord.BlacklistID }, new
                {
                    message = $"用户 '{user.Username}' 已加入黑名单",
                    userId = dto.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加入黑名单失败");
                return StatusCode(500, new { message = "加入黑名单失败" });
            }
        }

        /// <summary>
        /// 解除黑名单（仅管理员）
        /// </summary>
        [HttpPatch("{id}/release")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Release(string id, [FromBody] ReleaseBlacklistDto? dto)
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            try
            {
                // 检查黑名单记录是否存在
                var record = await _blacklistRepository.GetByIdAsync(id);
                if (record == null)
                {
                    return NotFound(new { message = "黑名单记录不存在" });
                }

                // 检查是否已解除
                if (string.Equals(record.BlacklistStatus, "RELEASED", StringComparison.OrdinalIgnoreCase)) {
                    return Conflict(new { message = "该黑名单记录已被解除" });
                }

                // 获取当前操作人ID
                var operatorId = CurrentOperatorId();
                if (operatorId == null) return Unauthorized(new { message = "登录状态无效，请重新登录。" });

                // 解除黑名单
                await _blacklistRepository.ReleaseAsync(id, operatorId);

                return Ok(new
                {
                    message = "黑名单已解除",
                    blacklistId = id,
                    releasedBy = operatorId,
                    releaseTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解除黑名单失败");
                return StatusCode(500, new { message = "解除黑名单失败" });
            }
        }

        /// <summary>
        /// 查询用户有效黑名单状态（供领养审核模块调用）
        /// </summary>
        [HttpGet("status/{userId}")]
        [Authorize(Roles = "ADMIN,VOLUNTEER")]
        [ProducesResponseType(typeof(BlacklistStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserBlacklistStatus(string userId)
        {
            var denied = await EnsureBlacklistStatusAccessAsync();
            if (denied != null) return denied;

            if (string.IsNullOrWhiteSpace(userId)) {
                return BadRequest(new { message = "用户 ID 不能为空" });
            }

            try
            {
                // 检查用户是否存在
                var user = await _userRepository.GetById(userId);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 获取黑名单状态
                var status = await _blacklistRepository.GetActiveStatusByUserIdAsync(userId);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询用户黑名单状态失败");
                return StatusCode(500, new { message = "查询用户黑名单状态失败" });
            }
        }

        /// <summary>
        /// 批量解除黑名单（仅管理员）
        /// </summary>
        [HttpPatch("release/batch")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReleaseBatch([FromBody] ReleaseBatchDto dto)
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            if (!ModelState.IsValid || dto.BlacklistIds == null || dto.BlacklistIds.Count == 0)
            {
                return BadRequest(new { message = "请提供要解除的黑名单ID列表" });
            }

            try
            {
                var operatorId = CurrentOperatorId();
                if (operatorId == null) return Unauthorized(new { message = "登录状态无效，请重新登录。" });
                var successList = new List<string>();
                var failList = new List<string>();

                foreach (var blacklistId in dto.BlacklistIds)
                {
                    try
                    {
                        // 检查记录是否存在
                        var record = await _blacklistRepository.GetByIdAsync(blacklistId);
                        if (record == null)
                        {
                            failList.Add($"{blacklistId} (记录不存在)");
                            continue;
                        }

                        // 检查是否已解除
                        if (string.Equals(record.BlacklistStatus, "RELEASED", StringComparison.OrdinalIgnoreCase)) {
                            failList.Add($"{blacklistId} (已解除)");
                            continue;
                        }

                        await _blacklistRepository.ReleaseAsync(blacklistId, operatorId);
                        successList.Add(blacklistId);
                    }
                    catch
                    {
                        failList.Add($"{blacklistId} (解除失败)");
                    }
                }

                return Ok(new
                {
                    message = $"成功解除 {successList.Count} 条记录",
                    success = successList,
                    failed = failList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量解除黑名单失败");
                return StatusCode(500, new { message = "批量解除黑名单失败" });
            }
        }

        private string? CurrentOperatorId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private async Task<ActionResult?> EnsureAdminAccessAsync()
        {
            var userId = CurrentOperatorId();
            if (userId == null) return Unauthorized();

            var user = await _userRepository.GetById(userId);
            if (user == null || !UserStatusCodes.IsActive(user.Status)) return Unauthorized();

            var isAdmin = string.Equals(user.RoleName, "ADMIN", StringComparison.OrdinalIgnoreCase)
                || HasPermission(user.PermissionScope, "BLACKLIST_MANAGE");
            return isAdmin ? null : Forbid();
        }

        private async Task<ActionResult?> EnsureBlacklistStatusAccessAsync()
        {
            var userId = CurrentOperatorId();
            if (userId == null) return Unauthorized();

            var user = await _userRepository.GetById(userId);
            if (user == null || !UserStatusCodes.IsActive(user.Status)) return Unauthorized();

            var roleName = user.RoleName ?? string.Empty;
            var canView = roleName.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) || roleName.Equals("VOLUNTEER", StringComparison.OrdinalIgnoreCase);
            return canView ? null : Forbid();
        }

        private static bool HasPermission(string? permissionScope, string requiredPermission) =>
            (permissionScope ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(permission => permission.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 分页结果类
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// 批量解除DTO
    /// </summary>
    public class ReleaseBatchDto
    {
        public List<string> BlacklistIds { get; set; }
    }
}
