using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cat.Models;
using Cat.Repositories;

namespace Cat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;

        public RolesController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            var roles = await _roleRepository.GetAll();

            if (roles == null || !roles.Any())
            {
                return NotFound("没有找到任何角色数据。");
            }

            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _roleRepository.GetByIdRole(id);

            if (role == null)
            {
                return NotFound($"未找到 ID 为 {id} 的角色。");
            }

            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
        {
            if (role == null)
            {
                return BadRequest("请求体为空，无法创建角色。");
            }

            await _roleRepository.CreateRole(role);
            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            if (role == null)
            {
                return BadRequest("请求体为空，无法更新角色。");
            }

            if (id != role.RoleID)
            {
                return BadRequest("URL 中的 ID 与请求体中的 ID 不匹配。");
            }

            var existing = await _roleRepository.GetByIdRole(id);
            if (existing == null)
            {
                return NotFound($"未找到 ID 为 {id} 的角色，无法更新。");
            }

            await _roleRepository.UpdateRole(role);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var existing = await _roleRepository.GetByIdRole(id);
            if (existing == null)
            {
                return NotFound($"未找到 ID 为 {id} 的角色，无法删除。");
            }

            await _roleRepository.DeleteRole(id);
            return NoContent();
        }
    }
}