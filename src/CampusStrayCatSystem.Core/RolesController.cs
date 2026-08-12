using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Models.DTOs;
using CampusStrayCatSystem.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CampusStrayCatSystem.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public RolesController(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            var roles = await _roleRepository.GetAll();

            if (roles == null || !roles.Any())
            {
                return NotFound("未找到任何角色数据。");
            }

            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(string id)
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            var role = await _roleRepository.GetByIdRole(id);

            if (role == null)
            {
                return NotFound($"未找到 ID 为 {id} 的角色。");
            }

            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)  // ✅ 改为 DTO
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            // 1. 校验
            if (dto == null || string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new { message = "角色名称不能为空" });
    
            var roleName = dto.RoleName.Trim();
    
             // ✅ 新增：校验角色名是否合法
            if (!RoleCodes.IsValid(roleName))
            {
                return BadRequest($"角色名必须是 {string.Join("、", RoleCodes.Allowed)}。");
            }

            // 2. 检查角色名是否重复（忽略大小写）
            if (await _roleRepository.ExistsByNameAsync(roleName))
                return Conflict(new { message = $"角色名称 '{roleName}' 已存在" });
    
            // 3. 生成 RoleID
            var roleId = Guid.NewGuid().ToString().ToLower();
    
            // 4. 创建角色
            var role = new Role
            {
                RoleID = roleId,
                RoleName = roleName.ToUpperInvariant(),
                Description = dto.Description,
                PermissionScope = dto.PermissionScope
            };
    
            // 5. 获取操作人
            var operatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
            // 6. 执行创建（含审计日志）
            await _roleRepository.CreateRoleWithAuditAsync(role, operatorId);
    
            // 7. 返回 201 Created
            return CreatedAtAction(nameof(GetRole), new { id = roleId }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] Role role)
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            if (role == null)
            {
                return BadRequest("角色数据为空，无法更新角色。");
            }

            if (id != role.RoleID)
            {
                return BadRequest("URL 中的 ID 与请求体中的 ID 不匹配。");
            }

            if (!RoleCodes.IsValid(role.RoleName))
            {
                return BadRequest($"角色名必须是 {string.Join("、", RoleCodes.Allowed)}。");
            }

            role.RoleName = role.RoleName.Trim().ToUpperInvariant();

            var existing = await _roleRepository.GetByIdRole(id);
            if (existing == null)
            {
                return NotFound($"未找到 ID 为 {id} 的角色，无法更新。");
            }

            await _roleRepository.UpdateRole(role);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            var existing = await _roleRepository.GetByIdRole(id);
            if (existing == null)
            {
                return NotFound($"未找到 ID 为 {id} 的角色，无法删除。");
            }

            if (await _roleRepository.GetUserCount(id) > 0)
            {
                return Conflict(new { message = "该角色仍被用户使用，不能直接删除。" });
            }

            await _roleRepository.DeleteRole(id);
            return NoContent();
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto request)
        {
            var denied = await EnsureAdminAccessAsync();
            if (denied != null) return denied;

            // 1. 参数校验
            if (request == null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.RoleId))
                return BadRequest(new { message = "UserId 和 RoleId 不能为空" });

            var userId = request.UserId.Trim();
            var roleId = request.RoleId.Trim();
    
            // 2. 校验用户存在
            if (!await _userRepository.Exists(userId))
                return NotFound(new { message = "用户不存在" });
    
            // 3. 校验角色存在
            if (await _roleRepository.GetByIdRole(roleId) == null)
                return NotFound(new { message = "角色不存在" });
    
            // 4. 获取操作人
            var operatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
            // 5. 执行分配（含审计日志）
            var result = await _roleRepository.AssignRoleWithAuditAsync(userId, roleId, operatorId);
    
            if (!string.IsNullOrEmpty(result))
                return BadRequest(new { message = result });
    
            return NoContent();
        }

        private async Task<ActionResult?> EnsureAdminAccessAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var user = await _userRepository.GetById(userId);
            if (user == null || !UserStatusCodes.IsActive(user.Status)) return Unauthorized();

            var isAdmin = string.Equals(user.RoleName, "ADMIN", StringComparison.OrdinalIgnoreCase)
                || HasPermission(user.PermissionScope, "ROLE_MANAGE");
            return isAdmin ? null : Forbid();
        }

        private static bool HasPermission(string? permissionScope, string requiredPermission) =>
            (permissionScope ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(permission => permission.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase));
    }
}
