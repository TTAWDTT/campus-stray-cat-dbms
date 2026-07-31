using Microsoft.AspNetCore.Mvc;
using CampusStrayCatSystem.Models;
using CampusStrayCatSystem.Data;

namespace CampusStrayCatSystem.Core
{
    // 投喂记录控制器，对应数据库表 VOL_CHECKINS
    // 志愿者完成投喂任务后通过本接口记录完成情况，系统在同一事务中把对应任务标记为已完成
    [Route("api/feeding-records")]
    [ApiController]
    public class FeedingRecordsController : ControllerBase
    {
        private readonly IVolCheckInRepository _checkInRepository;
        private readonly IVolShiftRepository _shiftRepository;

        public FeedingRecordsController(
            IVolCheckInRepository checkInRepository,
            IVolShiftRepository shiftRepository)
        {
            _checkInRepository = checkInRepository;
            _shiftRepository = shiftRepository;
        }

        // 获取所有投喂记录（按签到时间倒序）
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VolCheckIn>>> GetAll()
        {
            var records = await _checkInRepository.GetAll();
            return Ok(records ?? new List<VolCheckIn>());
        }

        // 按记录 ID 获取单条投喂记录
        [HttpGet("{id}")]
        public async Task<ActionResult<VolCheckIn>> GetById(string id)
        {
            var record = await _checkInRepository.GetById(id);
            if (record == null)
                return NotFound($"未找到 ID 为 {id} 的投喂记录。");

            return Ok(record);
        }

        // 按投喂任务 ID 查询该任务的投喂历史记录
        [HttpGet("by-shift/{shiftId}")]
        public async Task<ActionResult<IEnumerable<VolCheckIn>>> GetByShift(string shiftId)
        {
            // 校验任务是否存在
            if (!await _shiftRepository.Exists(shiftId))
                return NotFound($"未找到 ID 为 {shiftId} 的投喂任务。");

            var records = await _checkInRepository.GetByShift(shiftId);
            return Ok(records ?? new List<VolCheckIn>());
        }

        // 按志愿者ID查询其所有投喂记录（“我的投喂历史”）
        [HttpGet("by-volunteer/{volunteerId}")]
        public async Task<ActionResult<IEnumerable<VolCheckIn>>> GetByVolunteer(string volunteerId)
        {
            var records = await _checkInRepository.GetByVolunteer(volunteerId);
            return Ok(records ?? new List<VolCheckIn>());
        }

        // 记录一次投喂完成情况：新增打卡记录，并把对应任务状态置为 COMPLETED
        [HttpPost]
        public async Task<ActionResult<VolCheckIn>> Create([FromBody] VolCheckIn record)
        {
            if (record == null)
                return BadRequest("投喂记录数据为空，无法创建。");

            // 校验关联的投喂任务存在
            if (string.IsNullOrWhiteSpace(record.ShiftID))
                return BadRequest("ShiftID 不能为空。");

            if (!await _shiftRepository.Exists(record.ShiftID))
                return BadRequest($"投喂任务 ShiftID='{record.ShiftID}' 不存在。");

            // 校验打卡状态合法性
            if (!string.IsNullOrWhiteSpace(record.CheckInStatus))
            {
                if (!CheckInStatuses.IsValid(record.CheckInStatus))
                    return BadRequest($"无效的打卡状态 '{record.CheckInStatus}'。允许的状态: {string.Join(", ", CheckInStatuses.Allowed)}");
            }

            if (record.Longitude is < -180 or > 180)
                return BadRequest("经度 Longitude 必须在 -180 到 180 之间。");

            if (record.Latitude is < -90 or > 90)
                return BadRequest("纬度 Latitude 必须在 -90 到 90 之间。");

            if (record.DistanceMeters < 0)
                return BadRequest("距离 DistanceMeters 不能为负数。");

            // 事务性写入：记录投喂 + 标记任务完成
            var created = await _checkInRepository.CreateWithShiftCompleted(record);
            if (!created)
                return Conflict("该投喂任务已经完成、已有打卡记录或当前状态不允许打卡。");

            return CreatedAtAction(nameof(GetById), new { id = record.CheckInID }, record);
        }
    }
}
