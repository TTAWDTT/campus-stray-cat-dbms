using Microsoft.AspNetCore.Mvc;
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
    public class MedReminderController : ControllerBase
    {
        private readonly IMedReminderRepository _reminderRepository;

        public MedReminderController(IMedReminderRepository reminderRepository)
        {
            _reminderRepository = reminderRepository;
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
            var rows = await _reminderRepository.MarkSent(reminderId);
            return rows == 0 ? NotFound($"未找到提醒 {reminderId}。") : NoContent();
        }

        /// <summary>
        /// 把提醒标记为已完成。
        /// 这一步表示后续护理动作已经处理完毕。
        /// </summary>
        [HttpPut("{reminderId}/complete")]
        public async Task<IActionResult> Complete(string reminderId)
        {
            var rows = await _reminderRepository.Complete(reminderId);
            return rows == 0 ? NotFound($"未找到提醒 {reminderId}。") : NoContent();
        }
    }
}