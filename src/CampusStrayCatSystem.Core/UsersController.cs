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
            var users = await _userRepository.GetAll(NormalizeOptional(username), NormalizeOptional(status), NormalizeOptional(roleId));
            return Ok(users.Select(ToProfile));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileResponse>> GetUser(string id)
        {
            var user = await _userRepository.GetById(id);
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

            if (await _userRepository.UsernameExists(request.Username.Trim()))
            {
                return Conflict(new { message = $"用户名 {request.Username} 已存在。" });
            }

            var role = await _roleRepository.GetByIdRole(request.RoleID);
            if (role == null)
            {
                return BadRequest(new { message = $"未找到 ID 为 {request.RoleID} 的角色。" });
            }

            var user = new User
            {
                UserID = Guid.NewGuid().ToString(),
                RoleID = request.RoleID.Trim(),
                Username = request.Username.Trim(),
                RealName = NormalizeOptional(request.RealName),
                StudentNo = NormalizeOptional(request.StudentNo),
                Phone = NormalizeOptional(request.Phone),
                VerifyStatus = NormalizeOptional(request.VerifyStatus),
                Status = NormalizeStatus(request.Status) ?? "ACTIVE"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.Create(user);
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

            var existing = await _userRepository.GetById(id);
            if (existing == null)
            {
                return NotFound(new { message = $"未找到 ID 为 {id} 的用户。" });
            }

            var role = await _roleRepository.GetByIdRole(request.RoleID);
            if (role == null)
            {
                return BadRequest(new { message = $"未找到 ID 为 {request.RoleID} 的角色。" });
            }

            existing.RoleID = request.RoleID.Trim();
            existing.RealName = NormalizeOptional(request.RealName);
            existing.StudentNo = NormalizeOptional(request.StudentNo);
            existing.Phone = NormalizeOptional(request.Phone);
            existing.VerifyStatus = NormalizeOptional(request.VerifyStatus);
            existing.Status = NormalizeStatus(request.Status) ?? existing.Status;

            await _userRepository.Update(existing);
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusRequest request)
        {
            if (!HasAdminAccess())
            {
                return Forbid();
            }

            if (!await _userRepository.Exists(id))
            {
                return NotFound(new { message = $"未找到 ID 为 {id} 的用户。" });
            }

            var normalizedStatus = NormalizeStatus(request.Status);
            if (normalizedStatus == null)
            {
                return BadRequest(new { message = "状态只能是 ACTIVE、DISABLED、ENABLED 或 正常/停用。" });
            }

            await _userRepository.UpdateStatus(id, normalizedStatus);
            return NoContent();
        }

        private bool HasAdminAccess()
        {
            var roleName = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ?? string.Empty;
            var permissionScope = User.FindFirst("permissionScope")?.Value ?? string.Empty;

            return roleName.Contains("ADMIN", StringComparison.OrdinalIgnoreCase) ||
                   roleName.Contains("管理员", StringComparison.OrdinalIgnoreCase) ||
                   permissionScope.Contains("USER_MANAGE", StringComparison.OrdinalIgnoreCase) ||
                   permissionScope.Contains("ROLE_MANAGE", StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return null;
            }

            var normalized = status.Trim().ToUpperInvariant();
            return normalized switch
            {
                "ACTIVE" => "ACTIVE",
                "ENABLED" => "ENABLED",
                "DISABLED" => "DISABLED",
                "正常" => "正常",
                "停用" => "DISABLED",
                _ => null
            };
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
