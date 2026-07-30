using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedHealthRecordsController : ControllerBase
    {
        private readonly IMedHealthRecordRepository _medHealthRecordRepository;

        public MedHealthRecordsController(IMedHealthRecordRepository medHealthRecordRepository)
        {
            _medHealthRecordRepository = medHealthRecordRepository;
        }

        //获取所有医疗记录
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedHealthRecord>>> GetAll()
        {
            var records = await _medHealthRecordRepository.GetAll();

            if (records == null || !records.Any())
                return NotFound("未找到任何医疗记录数据。");

            return Ok(records);
        }

        //按猫咪ID查询医疗历史
        [HttpGet("cat/{catId}")]
        public async Task<ActionResult<IEnumerable<MedHealthRecord>>> GetByCatId(string catId)
        {
            var records = await _medHealthRecordRepository.GetByCatId(catId);

            if (records == null || !records.Any())
                return Ok(new List<MedHealthRecord>()); // 返回空数组

            return Ok(records);
        }

        //根据ID获取单条医疗记录
        [HttpGet("{id}")]
        public async Task<ActionResult<MedHealthRecord>> GetById(string id)
        {
            var record = await _medHealthRecordRepository.GetById(id);

            if (record == null)
                return NotFound($"未找到 ID 为 {id} 的医疗记录。");

            return Ok(record);
        }

        //新增医疗记录
        [HttpPost]
        public async Task<ActionResult<MedHealthRecord>> Create([FromBody] MedHealthRecord record)
        {
            if (record == null)
                return BadRequest("医疗记录数据为空，无法创建。");

            await _medHealthRecordRepository.Create(record);
            return CreatedAtAction(nameof(GetById), new { id = record.RecordID }, record);
        }

        //编辑医疗记录
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MedHealthRecord record)
        {
            if (record == null)
                return BadRequest("医疗记录数据为空，无法更新。");

            if (id != record.RecordID)
                return BadRequest("URL 中的 ID 与请求体中的 ID 不匹配。");

            var existing = await _medHealthRecordRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的医疗记录，无法更新。");

            await _medHealthRecordRepository.Update(record);
            return NoContent();
        }

        //删除医疗记录
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _medHealthRecordRepository.GetById(id);
            if (existing == null)
                return NotFound($"未找到 ID 为 {id} 的医疗记录，无法删除。");

            await _medHealthRecordRepository.Delete(id);
            return NoContent();
        }
    }
}
