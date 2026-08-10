using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Core
{
    // 医疗提醒接口
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,VOLUNTEER,VET")]
    public class MedReminderController : ControllerBase
    {
        private readonly IMedReminderRepository _reminderRepository;
        private readonly ICatRepository _catRepository;

        public MedReminderController(
            IMedReminderRepository reminderRepository,
            ICatRepository catRepository)
        {
            _reminderRepository = reminderRepository;
            _catRepository = catRepository;
        }

        // 获取待处理或已发送的提醒列表
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedReminder>>> GetPendingReminders()
        {
            var reminders = await _reminderRepository.GetPendingReminders();
            return Ok(reminders);
        }

        // 按猫咪查询提醒历史
        [HttpGet("cat/{catId}")]
        public async Task<ActionResult<IEnumerable<MedReminder>>> GetByCatId(string catId)
        {
            if (string.IsNullOrWhiteSpace(catId))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            var reminders = await _reminderRepository.GetByCatId(catId);
            return Ok(reminders);
        }

        // 新增一条提醒
        [HttpPost]
        public async Task<ActionResult<MedReminder>> Create([FromBody] MedReminder reminder)
        {
            if (reminder == null)
            {
                return BadRequest("提醒数据不能为空。");
            }

            if (string.IsNullOrWhiteSpace(reminder.CatID))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            if (!await _catRepository.Exists(reminder.CatID))
                return NotFound($"未找到 ID 为 {reminder.CatID} 的猫咪档案。");

            if (string.IsNullOrWhiteSpace(reminder.ReminderType) ||
                !ReminderTypes.IsValid(reminder.ReminderType))
            {
                return BadRequest($"提醒类型必须是 {string.Join("、", ReminderTypes.Allowed)}。");
            }

            if (!string.IsNullOrWhiteSpace(reminder.SendStatus) && !ReminderStatuses.IsValid(reminder.SendStatus))
            {
                return BadRequest($"发送状态必须是 {string.Join("、", ReminderStatuses.Allowed)}。");
            }

            if (reminder.ReminderTime == null)
            {
                return BadRequest("提醒时间不能为空。");
            }

            reminder.ReminderType = reminder.ReminderType.Trim().ToUpperInvariant();
            reminder.SendStatus = string.IsNullOrWhiteSpace(reminder.SendStatus)
                ? ReminderStatuses.Pending
                : reminder.SendStatus.Trim().ToUpperInvariant();
            await _reminderRepository.CreateReminder(reminder);
            return CreatedAtAction(nameof(GetById), new { reminderId = reminder.ReminderID }, reminder);
        }

        // 查看提醒详情
        [HttpGet("{reminderId}")]
        public async Task<ActionResult<MedReminder>> GetById(string reminderId)
        {
            if (string.IsNullOrWhiteSpace(reminderId))
            {
                return BadRequest("提醒 ID 不能为空。");
            }

            var reminder = await _reminderRepository.GetById(reminderId);
            return reminder == null ? NotFound($"未找到提醒 {reminderId}。") : Ok(reminder);
        }

        // 标记提醒已发送
        [HttpPut("{reminderId}/sent")]
        public async Task<IActionResult> MarkSent(string reminderId)
        {
            if (string.IsNullOrWhiteSpace(reminderId))
            {
                return BadRequest("提醒 ID 不能为空。");
            }

            if (await _reminderRepository.GetById(reminderId) == null)
            {
                return NotFound($"未找到提醒 {reminderId}。");
            }

            var rows = await _reminderRepository.MarkSent(reminderId);
            return NoContent();
        }

        // 标记提醒已完成
        [HttpPut("{reminderId}/complete")]
        public async Task<IActionResult> Complete(string reminderId)
        {
            if (string.IsNullOrWhiteSpace(reminderId))
            {
                return BadRequest("提醒 ID 不能为空。");
            }

            if (await _reminderRepository.GetById(reminderId) == null)
            {
                return NotFound($"未找到提醒 {reminderId}。");
            }

            var rows = await _reminderRepository.Complete(reminderId);
            return NoContent();
        }
    }
}
