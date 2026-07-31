using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    // 志愿者交接记录数据访问实现，对应数据库表 VOL_HANDOVERS
    public class VolHandoverRepository : BaseRepository<VolHandover>, IVolHandoverRepository
    {
        public VolHandoverRepository(IConfiguration configuration) : base(configuration) { }

        // 获取所有交接记录，按发起时间倒序
        public async Task<IEnumerable<VolHandover>> GetAll()
        {
            const string sql = @"
                SELECT HANDOVERID AS HandoverID,
                       FROMVOLUNTEERID AS FromVolunteerID,
                       TOVOLUNTEERID AS ToVolunteerID,
                       RELATEDTYPE AS RelatedType,
                       RELATEDID AS RelatedID,
                       APPLYTIME AS ApplyTime,
                       CONFIRMTIME AS ConfirmTime,
                       HANDOVERSTATUS AS HandoverStatus,
                       REMARK AS Remark
                FROM VOL_HANDOVERS
                ORDER BY APPLYTIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        // 按交接 ID 获取单条交接记录
        public async Task<VolHandover?> GetById(string handoverId)
        {
            const string sql = @"
                SELECT HANDOVERID AS HandoverID,
                       FROMVOLUNTEERID AS FromVolunteerID,
                       TOVOLUNTEERID AS ToVolunteerID,
                       RELATEDTYPE AS RelatedType,
                       RELATEDID AS RelatedID,
                       APPLYTIME AS ApplyTime,
                       CONFIRMTIME AS ConfirmTime,
                       HANDOVERSTATUS AS HandoverStatus,
                       REMARK AS Remark
                FROM VOL_HANDOVERS
                WHERE HANDOVERID = :HandoverID";

            return await QuerySingleAsync(sql, new { HandoverID = handoverId });
        }

        // 查询某志愿者发起的所有交接
        public async Task<IEnumerable<VolHandover>> GetByFromVolunteer(string fromVolunteerId)
        {
            const string sql = @"
                SELECT HANDOVERID AS HandoverID,
                       FROMVOLUNTEERID AS FromVolunteerID,
                       TOVOLUNTEERID AS ToVolunteerID,
                       RELATEDTYPE AS RelatedType,
                       RELATEDID AS RelatedID,
                       APPLYTIME AS ApplyTime,
                       CONFIRMTIME AS ConfirmTime,
                       HANDOVERSTATUS AS HandoverStatus,
                       REMARK AS Remark
                FROM VOL_HANDOVERS
                WHERE FROMVOLUNTEERID = :FromVolunteerID
                ORDER BY APPLYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { FromVolunteerID = fromVolunteerId });
        }

        // 查询某志愿者需要确认的所有交接
        public async Task<IEnumerable<VolHandover>> GetByToVolunteer(string toVolunteerId)
        {
            const string sql = @"
                SELECT HANDOVERID AS HandoverID,
                       FROMVOLUNTEERID AS FromVolunteerID,
                       TOVOLUNTEERID AS ToVolunteerID,
                       RELATEDTYPE AS RelatedType,
                       RELATEDID AS RelatedID,
                       APPLYTIME AS ApplyTime,
                       CONFIRMTIME AS ConfirmTime,
                       HANDOVERSTATUS AS HandoverStatus,
                       REMARK AS Remark
                FROM VOL_HANDOVERS
                WHERE TOVOLUNTEERID = :ToVolunteerID
                ORDER BY APPLYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { ToVolunteerID = toVolunteerId });
        }

        // 按状态筛选交接记录
        public async Task<IEnumerable<VolHandover>> GetByStatus(string status)
        {
            const string sql = @"
                SELECT HANDOVERID AS HandoverID,
                       FROMVOLUNTEERID AS FromVolunteerID,
                       TOVOLUNTEERID AS ToVolunteerID,
                       RELATEDTYPE AS RelatedType,
                       RELATEDID AS RelatedID,
                       APPLYTIME AS ApplyTime,
                       CONFIRMTIME AS ConfirmTime,
                       HANDOVERSTATUS AS HandoverStatus,
                       REMARK AS Remark
                FROM VOL_HANDOVERS
                WHERE HANDOVERSTATUS = :HandoverStatus
                ORDER BY APPLYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { HandoverStatus = status });
        }

        // 按关联对象查询交接历史，例如查询某投喂任务被交接过几次
        public async Task<IEnumerable<VolHandover>> GetByRelated(string relatedType, string relatedId)
        {
            const string sql = @"
                SELECT HANDOVERID AS HandoverID,
                       FROMVOLUNTEERID AS FromVolunteerID,
                       TOVOLUNTEERID AS ToVolunteerID,
                       RELATEDTYPE AS RelatedType,
                       RELATEDID AS RelatedID,
                       APPLYTIME AS ApplyTime,
                       CONFIRMTIME AS ConfirmTime,
                       HANDOVERSTATUS AS HandoverStatus,
                       REMARK AS Remark
                FROM VOL_HANDOVERS
                WHERE RELATEDTYPE = :RelatedType
                  AND RELATEDID = :RelatedID
                ORDER BY APPLYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { RelatedType = relatedType, RelatedID = relatedId });
        }

        // 提交交接：生成主键，状态默认 PENDING，发起时间记为当前时间
        public async Task<int> Create(VolHandover handover)
        {
            handover.HandoverID = Guid.NewGuid().ToString();
            handover.HandoverStatus = HandoverStatuses.Pending;
            handover.ApplyTime = DateTime.Now;

            const string sql = @"
                INSERT INTO VOL_HANDOVERS (HANDOVERID, FROMVOLUNTEERID, TOVOLUNTEERID,
                                           RELATEDTYPE, RELATEDID, APPLYTIME, CONFIRMTIME,
                                           HANDOVERSTATUS, REMARK)
                VALUES (:HandoverID, :FromVolunteerID, :ToVolunteerID,
                        :RelatedType, :RelatedID, :ApplyTime, :ConfirmTime,
                        :HandoverStatus, :Remark)";

            return await ExecuteAsync(sql, new
            {
                handover.HandoverID,
                handover.FromVolunteerID,
                handover.ToVolunteerID,
                handover.RelatedType,
                handover.RelatedID,
                handover.ApplyTime,
                handover.ConfirmTime,
                handover.HandoverStatus,
                handover.Remark
            });
        }

        // 确认交接（事务）：1) 更新状态为 CONFIRMED，写入确认时间；2) 若关联投喂任务，把任务负责人改为接收方
        // relatedType/relatedId 由调用方传入，避免事务内重复查询
        public async Task Confirm(string handoverId, string toVolunteerId, string? relatedType, string? relatedId)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 更新交接状态为已确认，记录确认时间
                const string updateHandoverSql = @"
                    UPDATE VOL_HANDOVERS
                    SET HANDOVERSTATUS = :HandoverStatus,
                        CONFIRMTIME = SYSDATE
                    WHERE HANDOVERID = :HandoverID";

                await ExecuteAsync(connection, transaction, updateHandoverSql, new
                {
                    HandoverStatus = HandoverStatuses.Confirmed,
                    HandoverID = handoverId
                });

                // 若是投喂任务交接，则把任务负责人改为接收方志愿者
                if (string.Equals(relatedType, "SHIFT", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(relatedId))
                {
                    const string updateShiftSql = @"
                        UPDATE VOL_SHIFTS
                        SET VOLUNTEERID = :VolunteerID
                        WHERE SHIFTID = :ShiftID";

                    await ExecuteAsync(connection, transaction, updateShiftSql, new
                    {
                        VolunteerID = toVolunteerId,
                        ShiftID = relatedId
                    });
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // 拒绝交接：仅更新状态
        public async Task<int> Reject(string handoverId)
        {
            const string sql = @"
                UPDATE VOL_HANDOVERS
                SET HANDOVERSTATUS = :HandoverStatus
                WHERE HANDOVERID = :HandoverID";

            return await ExecuteAsync(sql, new
            {
                HandoverStatus = HandoverStatuses.Rejected,
                HandoverID = handoverId
            });
        }

        // 撤销交接：仅更新状态
        public async Task<int> Cancel(string handoverId)
        {
            const string sql = @"
                UPDATE VOL_HANDOVERS
                SET HANDOVERSTATUS = :HandoverStatus
                WHERE HANDOVERID = :HandoverID";

            return await ExecuteAsync(sql, new
            {
                HandoverStatus = HandoverStatuses.Cancelled,
                HandoverID = handoverId
            });
        }
    }
}
