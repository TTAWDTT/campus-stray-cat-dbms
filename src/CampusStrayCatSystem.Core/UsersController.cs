using System.Security.Claims;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersController(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfileResponse>>> GetUsers(
            [FromQuery] string? username = null,
            [FromQuery] string? status = null,
            [FromQuery] string? roleId = null)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = NormalizeStatus(status);
                if (normalizedStatus == null)
                {
                    return BadRequest(new { message = "status 仅支持 ACTIVE 或 DISABLED。" });
                }

                status = normalizedStatus;
            }

            if (!string.IsNullOrWhiteSpace(roleId) && string.IsNullOrWhiteSpace(roleId.Trim()))
            {
                return BadRequest(new { message = "roleId 不能为空字符串。" });
            }

            var users = await _userRepository.GetAll(
                NormalizeOptional(username),
                NormalizeOptional(status),
                NormalizeOptional(roleId));
            return Ok(users.Select(ToProfile));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileResponse>> GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { message = "用户 ID 不能为空。" });
            }

            var user = await _userRepository.GetById(id.Trim());
            if (user == null)
            {
                return NotFound(new { message = $"未找到 ID 为 {id} 的用户。" });
            }

            return Ok(ToProfile(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!HasAdminAccess())
            {
                return Forbid();
            }

            if (request == null)
            {
                return BadRequest(new { message = "请求体不能为空。" });
            }

            var username = request.Username?.Trim();
            var roleId = request.RoleID?.Trim();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username、Password、RoleID 均为必填。" });
            }

            var status = NormalizeStatus(request.Status) ?? UserStatusCodes.Active;
            if (!UserVerifyStatusCodes.IsKnown(request.VerifyStatus))
            {
                return BadRequest(new { message = "VerifyStatus 仅支持 VERIFIED、UNVERIFIED 或空。" });
            }

            if (await _userRepository.UsernameExists(username))
            {
                return Conflict(new { message = $"用户名 {username} 已存在。" });
            }

            var role = await _roleRepository.GetByIdRole(roleId);
            if (role == null)
            {
                return BadRequest(new { message = $"未找到 ID 为 {roleId} 的角色。" });
            }

            // UserID / PasswordHash 由服务端生成，忽略客户端可能夹带的同类字段。
            var user = new User
            {
                UserID = Guid.NewGuid().ToString(),
                RoleID = roleId,
                Username = username,
                RealName = NormalizeOptional(request.RealName),
                StudentNo = NormalizeOptional(request.StudentNo),
                Phone = NormalizeOptional(request.Phone),
                VerifyStatus = NormalizeOptional(request.VerifyStatus) ?? UserVerifyStatusCodes.Unverified,
                Status = status
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            var affected = await _userRepository.Create(user);
            if (affected <= 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "新增用户失败。" });
            }

            var created = await _userRepository.GetById(user.UserID);
            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, ToProfile(created ?? user));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            if (!HasAdminAccess())
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { message = "用户 ID 不能为空。" });
            }

            if (request == null)
            {
                return BadRequest(new { message = "请求体不能为空。" });
            }

            var existing = await _userRepository.GetById(id.Trim());
            if (existing == null)
            {
                return NotFound(new { message = $"未找到 ID 为 {id} 的用户。" });
            }

            var roleId = request.RoleID?.Trim();
            if (string.IsNullOrWhiteSpace(roleId))
            {
                return BadRequest(new { message = "RoleID 不能为空。" });
            }

            if (!UserVerifyStatusCodes.IsKnown(request.VerifyStatus))
            {
                return BadRequest(new { message = "VerifyStatus 仅支持 VERIFIED、UNVERIFIED 或空。" });
            }

            var role = await _roleRepository.GetByIdRole(roleId);
            if (role == null)
            {
                return BadRequest(new { message = $"未找到 ID 为 {roleId} 的角色。" });
            }

            // 保护 Username / PasswordHash / UserID：本接口不接受改写。
            existing.RoleID = roleId;
            existing.RealName = NormalizeOptional(request.RealName);
            existing.StudentNo = NormalizeOptional(request.StudentNo);
            existing.Phone = NormalizeOptional(request.Phone);
            existing.VerifyStatus = NormalizeOptional(request.VerifyStatus) ?? existing.VerifyStatus;

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = NormalizeStatus(request.Status);
                if (status == null)
                {
                    return BadRequest(new { message = "Status 仅支持 ACTIVE 或 DISABLED。" });
                }

                existing.Status = status;
            }

            var affected = await _userRepository.Update(existing);
            if (affected <= 0)
            {
                return Conflict(new { message = "用户更新未生效，请刷新后重试。" });
            }

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusRequest request)
        {
            if (!HasAdminAccess())
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { message = "用户 ID 不能为空。" });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Status 不能为空。" });
            }

            if (!await _userRepository.Exists(id.Trim()))
            {
                return NotFound(new { message = $"未找到 ID 为 {id} 的用户。" });
            }

            var normalizedStatus = NormalizeStatus(request.Status);
            if (normalizedStatus == null)
            {
                return BadRequest(new { message = "Status 仅支持 ACTIVE 或 DISABLED。" });
            }

            var affected = await _userRepository.UpdateStatus(id.Trim(), normalizedStatus);
            if (affected <= 0)
            {
                return Conflict(new { message = "用户状态更新未生效，请刷新后重试。" });
            }

            return NoContent();
        }

        private bool HasAdminAccess()
        {
            var roleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var permissionScope = User.FindFirst("permissionScope")?.Value ?? string.Empty;

            return roleName.Equals("ADMIN", StringComparison.OrdinalIgnoreCase) ||
                   permissionScope.Contains("USER_MANAGE", StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeOptional(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return null;
            }

            var normalized = status.Trim().ToUpperInvariant();
            return UserStatusCodes.IsKnown(normalized) ? normalized : null;
        }

        private static UserProfileResponse ToProfile(User user) => new()
        {
            UserID = user.UserID,
            RoleID = user.RoleID,
            Username = user.Username,
            RealName = user.RealName,
            StudentNo = user.StudentNo,
            Phone = user.Phone,
            VerifyStatus = user.VerifyStatus,
            Status = user.Status,
            RoleName = user.RoleName,
            PermissionScope = user.PermissionScope
        };
    }
}
