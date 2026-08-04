using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CampusStrayCatSystem.Core
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(IUserRepository userRepository, IConfiguration configuration, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepository.GetByUsername(request.Username.Trim());
            if (user == null)
            {
                return Unauthorized(new { message = "用户名或密码错误。" });
            }

            if (!IsUserActive(user.Status))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "当前账号已停用，无法登录。" });
            }

            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "用户名或密码错误。" });
            }

            var expiresAtUtc = DateTime.UtcNow.AddHours(8);
            var token = BuildToken(user, expiresAtUtc);

            return Ok(new LoginResponse
            {
                UserID = user.UserID,
                Username = user.Username,
                RealName = user.RealName,
                RoleID = user.RoleID,
                RoleName = user.RoleName,
                PermissionScope = user.PermissionScope,
                Token = token,
                ExpiresAtUtc = expiresAtUtc
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileResponse>> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "登录状态无效，请重新登录。" });
            }

            var user = await _userRepository.GetById(userId);
            if (user == null)
            {
                return NotFound(new { message = "当前登录用户不存在。" });
            }

            return Ok(ToProfile(user));
        }

        [Authorize]
        [HttpGet("check")]
        public async Task<ActionResult<object>> Check()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { authenticated = false, message = "未检测到有效登录状态。" });
            }

            var user = await _userRepository.GetById(userId);
            if (user == null || !IsUserActive(user.Status))
            {
                return Unauthorized(new { authenticated = false, message = "当前登录状态已失效。" });
            }

            return Ok(new
            {
                authenticated = true,
                user = ToProfile(user)
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "服务端已完成退出响应。JWT 为无状态令牌，如需立即失效请前端清除本地 Token。" });
        }

        private string BuildToken(User user, DateTime expiresAtUtc)
        {
            var secret = _configuration["Auth:JwtSecret"] ?? _configuration["AUTH_JWT_SECRET"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("缺少 Auth:JwtSecret，无法签发 Token。");
            }

            var issuer = _configuration["Auth:Issuer"] ?? "CampusStrayCatSystem";
            var audience = _configuration["Auth:Audience"] ?? "CampusStrayCatSystemClient";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserID),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.RoleName ?? string.Empty),
                new("roleId", user.RoleID),
                new("permissionScope", user.PermissionScope ?? string.Empty)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool IsUserActive(string? status) =>
            !string.IsNullOrWhiteSpace(status) &&
            (status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) ||
             status.Equals("ENABLED", StringComparison.OrdinalIgnoreCase) ||
             status.Equals("正常", StringComparison.OrdinalIgnoreCase));

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
