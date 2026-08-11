using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Core
{
    /// <summary>
    /// 医疗提醒接口。
    /// 负责创建提醒、查看待处理提醒，以及更新提醒发送状态。
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,VOLUNTEER,VET")]
    public class MedReminderController : ControllerBase
    {
        private readonly IMedReminderRepository _reminderRepository;
        private readonly ICatRepository _catRepository;
        private readonly IMedHealthRecordRepository _healthRecordRepository;
        private readonly IUserRepository _userRepository;

        public MedReminderController(
            IMedReminderRepository reminderRepository,
            ICatRepository catRepository,
            IMedHealthRecordRepository healthRecordRepository,
            IUserRepository userRepository)
        {
            _reminderRepository = reminderRepository;
            _catRepository = catRepository;
            _healthRecordRepository = healthRecordRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 获取待处理或已发送的提醒列表。
        /// 这个接口适合做提醒中心页面。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedReminder>>> GetPendingReminders()
        {
            var reminders = await _reminderRepository.GetPendingReminders();
            return Ok(reminders);
        }

        /// <summary>
        /// 按猫咪查询提醒历史。
        /// 这样可以在猫咪详情页直接看到后续护理安排。
        /// </summary>
        [HttpGet("cat/{catId}")]
        public async Task<ActionResult<IEnumerable<MedReminder>>> GetByCatId(string catId)
        {
            if (string.IsNullOrWhiteSpace(catId))
            {
                return BadRequest("猫咪 ID 不能为空。");
            }

            if (!await _catRepository.Exists(catId.Trim()))
            {
                return NotFound($"未找到猫咪 {catId}。");
            }

            var reminders = await _reminderRepository.GetByCatId(catId);
            return Ok(reminders);
        }

        /// <summary>
        /// 新增一条提醒。
        /// 前端把医疗记录、猫咪、接收人和提醒时间传进来即可。
        /// </summary>
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

            reminder.CatID = reminder.CatID.Trim();
            if (!await _catRepository.Exists(reminder.CatID))
            {
                return NotFound($"未找到猫咪 {reminder.CatID}。");
            }

            if (!string.IsNullOrWhiteSpace(reminder.RecordID))
            {
                reminder.RecordID = reminder.RecordID.Trim();
                var record = await _healthRecordRepository.GetById(reminder.RecordID);
                if (record == null)
                {
                    return NotFound($"未找到医疗记录 {reminder.RecordID}。");
                }

                if (!string.Equals(record.CatID, reminder.CatID, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("医疗记录不属于该猫咪。");
                }
            }

            if (!string.IsNullOrWhiteSpace(reminder.ReceiverUserID))
            {
                reminder.ReceiverUserID = reminder.ReceiverUserID.Trim();
                if (!await _userRepository.Exists(reminder.ReceiverUserID))
                {
                    return NotFound($"未找到接收者 {reminder.ReceiverUserID}。");
                }
            }

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

        /// <summary>
        /// 查看提醒详情。
        /// 这个接口主要用于创建成功后回查或排查数据。
        /// </summary>
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

        /// <summary>
        /// 把提醒标记为已发送。
        /// 这一步通常表示消息已经发到接收人手里。
        /// </summary>
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

        /// <summary>
        /// 把提醒标记为已完成。
        /// 这一步表示后续护理动作已经处理完毕。
        /// </summary>
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
