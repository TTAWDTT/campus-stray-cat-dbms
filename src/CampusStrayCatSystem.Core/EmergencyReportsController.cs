using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Core
{
    /// <summary>
    /// 紧急救助上报接口。
    /// 提供提交、分配和处理状态更新的完整闭环。
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyReportsController : ControllerBase
    {
        private readonly IEmergencyReportRepository _reportRepository;

        public EmergencyReportsController(IEmergencyReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// 获取全部紧急上报。
        /// 适合管理员工作台按时间查看最新上报。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmergencyReport>>> GetAll()
        {
            var reports = await _reportRepository.GetAll();
            return Ok(reports);
        }

        /// <summary>
        /// 按上报编号查看单条记录。
        /// 便于提交后回查和处理时核对内容。
        /// </summary>
        [HttpGet("{reportId}")]
        public async Task<ActionResult<EmergencyReport>> GetById(string reportId)
        {
            var report = await _reportRepository.GetById(reportId);
            return report == null ? NotFound($"未找到上报 {reportId}。") : Ok(report);
        }

        /// <summary>
        /// 提交一条新的紧急救助上报。
        /// 这是普通用户最先使用的入口。
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EmergencyReport>> Create([FromBody] EmergencyReport report)
        {
            if (report == null)
            {
                return BadRequest("上报数据不能为空。");
            }

            await _reportRepository.Create(report);
            return CreatedAtAction(nameof(GetById), new { reportId = report.ReportID }, report);
        }

        /// <summary>
        /// 给上报分配处理人。
        /// 一般由管理员或志愿者在接单后调用。
        /// </summary>
        [HttpPut("{reportId}/assign")]
        public async Task<IActionResult> AssignHandler(string reportId, [FromBody] string? handlerUserId)
        {
            var rows = await _reportRepository.AssignHandler(reportId, handlerUserId);
            return rows == 0 ? NotFound($"未找到上报 {reportId}。") : NoContent();
        }

        /// <summary>
        /// 更新上报处理状态和结果。
        /// 这个接口负责把“已受理、处理中、已完成”等状态写回去。
        /// </summary>
        [HttpPut("{reportId}/status")]
        public async Task<IActionResult> UpdateStatus(string reportId, [FromBody] EmergencyReport report)
        {
            if (report == null)
            {
                return BadRequest("状态更新数据不能为空。");
            }

            var rows = await _reportRepository.UpdateStatus(reportId, report.ProcessStatus, report.ProcessResult);
            return rows == 0 ? NotFound($"未找到上报 {reportId}。") : NoContent();
        }
    }
}