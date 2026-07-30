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
        private readonly ICatRepository _catRepository;

        public MedHealthRecordsController(
            IMedHealthRecordRepository medHealthRecordRepository,
            ICatRepository catRepository)
        {
            _medHealthRecordRepository = medHealthRecordRepository;
            _catRepository = catRepository;
        }

        //获取所有医疗记录
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedHealthRecord>>> GetAll()
        {
            var records = await _medHealthRecordRepository.GetAll();
            return Ok(records ?? new List<MedHealthRecord>());
        }

        //按猫咪ID查询医疗历史
        [HttpGet("cat/{catId}")]
        public async Task<ActionResult<IEnumerable<MedHealthRecord>>> GetByCatId(string catId)
        {
            var records = await _medHealthRecordRepository.GetByCatId(catId);
            return Ok(records ?? new List<MedHealthRecord>());
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

            var validationError = await ValidateMedRecord(record);
            if (validationError != null)
                return BadRequest(validationError);

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

            var validationError = await ValidateMedRecord(record);
            if (validationError != null)
                return BadRequest(validationError);

            await _medHealthRecordRepository.Update(record);
            return NoContent();
        }

        // 业务校验
        private async Task<string?> ValidateMedRecord(MedHealthRecord record)
        {
            // CatID 非空且猫咪存在
            if (string.IsNullOrWhiteSpace(record.CatID))
                return "CatID 不能为空。";

            if (!await _catRepository.Exists(record.CatID))
                return $"猫咪 CatID='{record.CatID}' 不存在。";

            // 医疗类型合法
            if (!string.IsNullOrWhiteSpace(record.RecordType))
            {
                if (!MedRecordTypes.IsValid(record.RecordType))
                    return $"无效的医疗类型 '{record.RecordType}'。允许的类型: {string.Join(", ", MedRecordTypes.Allowed)}";
            }

            return null; // 校验通过
        }
    }
}
