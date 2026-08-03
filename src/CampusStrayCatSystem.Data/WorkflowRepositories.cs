using CampusStrayCatSystem.Models;
using Microsoft.Extensions.Configuration;

namespace CampusStrayCatSystem.Data
{
    // 领养和志愿者流程的数据访问层，直接调用数据库里的包和视图。
    public interface IAdoptionWorkflowRepository
    {
        // 领养流程的查询和写入操作。
        Task<IEnumerable<AdoptionPendingAppDto>> GetPendingApplicationsAsync();
        Task<int> SubmitApplicationAsync(AdoptionApplicationCreateRequest request);
        Task<int> ReviewApplicationAsync(string applicationId, AdoptionApplicationReviewRequest request);
        Task<int> CreateVisitAsync(string applicationId, AdoptionVisitCreateRequest request);
        Task<IEnumerable<AdoptionVisitSummaryDto>> GetVisitSummaryAsync();
    }

    public interface IVolunteerWorkflowRepository
    {
        // 志愿者流程的查询和写入操作。
        Task<IEnumerable<VolunteerActivityDto>> GetActivityAsync();
        Task<int> RegisterVolunteerAsync(VolunteerRegisterRequest request);
        Task<int> CreateShiftAsync(VolunteerShiftCreateRequest request);
        Task<int> CheckInShiftAsync(string shiftId, VolunteerCheckInRequest request, string operatorUserId);
        Task<int> AddCreditLogAsync(VolunteerCreditLogCreateRequest request);
    }

    public class AdoptionWorkflowRepository : BaseRepository<object>, IAdoptionWorkflowRepository
    {
        public AdoptionWorkflowRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<AdoptionPendingAppDto>> GetPendingApplicationsAsync()
        {
            // 直接读取数据库视图，避免在 C# 里重复拼接列表逻辑。
            const string sql = @"
                SELECT APPLICATIONID AS ApplicationId,
                       CATID AS CatId,
                       CATNAME AS CatName,
                       APPLICANTUSERID AS ApplicantUserId,
                       APPLICANTNAME AS ApplicantName,
                       APPLYTIME AS ApplyTime,
                       CURRENTSTATUS AS CurrentStatus,
                       REVIEWERUSERID AS ReviewerUserId,
                       AGREEMENTNO AS AgreementNo,
                       CONFIRMTIME AS ConfirmTime
                FROM VW_PENDING_ADOPTION_APPS
                ORDER BY APPLYTIME DESC NULLS LAST, APPLICATIONID";

            return await QueryAsync<AdoptionPendingAppDto>(sql);
        }

        public async Task<int> SubmitApplicationAsync(AdoptionApplicationCreateRequest request)
        {
            const string sql = @"BEGIN PKG_ADOPTION_WORKFLOW.submit_application(:CatId, :ApplicantUserId, :Status); END;";

            // 强制后端将新申请设为 PENDING，忽略客户端传入的 Status 字段以防止绕过审核流程。
            return await ExecuteAsync(sql, new
            {
                request.CatId,
                request.ApplicantUserId,
                Status = "PENDING"
            });
        }

        public async Task<int> ReviewApplicationAsync(string applicationId, AdoptionApplicationReviewRequest request)
        {
            const string sql = @"BEGIN PKG_ADOPTION_WORKFLOW.review_application(:ApplicationId, :ReviewerUserId, :Status, :AgreementNo, :ConfirmTime); END;";

            // 注意：Oracle 按位置绑定时参数顺序敏感，确保 ReviewerUserId 在 Status 之前传入。
            return await ExecuteAsync(sql, new
            {
                ApplicationId = applicationId,
                ReviewerUserId = request.ReviewerUserId,
                Status = request.Status,
                request.AgreementNo,
                ConfirmTime = request.ConfirmTime ?? DateTime.Now
            });
        }

        public async Task<int> CreateVisitAsync(string applicationId, AdoptionVisitCreateRequest request)
        {
            const string sql = @"BEGIN PKG_ADOPTION_WORKFLOW.create_visit(:ApplicationId, :VisitType, :VisitorUserId, :VisitTime, :Conclusion, :PassFlag); END;";

            return await ExecuteAsync(sql, new
            {
                ApplicationId = applicationId,
                request.VisitType,
                VisitTime = request.VisitTime ?? DateTime.Now,
                request.VisitorUserId,
                request.Conclusion,
                request.PassFlag
            });
        }

        public async Task<IEnumerable<AdoptionVisitSummaryDto>> GetVisitSummaryAsync()
        {
            // 统一从回访汇总视图返回数据，供页面直接展示。
            const string sql = @"
                SELECT VISITID AS VisitId,
                       APPLICATIONID AS ApplicationId,
                       CATID AS CatId,
                       VISITTYPE AS VisitType,
                       VISITTIME AS VisitTime,
                       VISITORUSERID AS VisitorUserId,
                       CONCLUSION AS Conclusion,
                       PASSFLAG AS PassFlag,
                       CURRENTSTATUS AS CurrentStatus
                FROM VW_ADOPTION_VISIT_SUMMARY
                ORDER BY VISITTIME DESC NULLS LAST, VISITID";

            return await QueryAsync<AdoptionVisitSummaryDto>(sql);
        }
    }

    public class VolunteerWorkflowRepository : BaseRepository<object>, IVolunteerWorkflowRepository
    {
        public VolunteerWorkflowRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<VolunteerActivityDto>> GetActivityAsync()
        {
            // 志愿者活动页需要的字段都由视图一次返回。
            const string sql = @"
                SELECT VOLUNTEERID AS VolunteerId,
                       USERID AS UserId,
                       USERNAME AS UserName,
                       ACTIVESTATUS AS ActiveStatus,
                       CREDITLEVEL AS CreditLevel,
                       SERVICESCORE AS ServiceScore,
                       SHIFTID AS ShiftId,
                       SHIFTSTATUS AS ShiftStatus,
                       PLANSTARTTIME AS PlanStartTime,
                       PLANENDTIME AS PlanEndTime
                FROM VW_VOLUNTEER_ACTIVITY
                ORDER BY USERNAME NULLS LAST, VOLUNTEERID";

            return await QueryAsync<VolunteerActivityDto>(sql);
        }

        public async Task<int> RegisterVolunteerAsync(VolunteerRegisterRequest request)
        {
            const string sql = @"BEGIN PKG_VOLUNTEER_MGMT.register_volunteer(:UserId, :JoinDate, :ServiceScore, :CreditLevel, :ActiveStatus, :GraduationYear); END;";

            return await ExecuteAsync(sql, new
            {
                request.UserId,
                JoinDate = request.JoinDate ?? DateTime.Now,
                request.ServiceScore,
                request.CreditLevel,
                request.ActiveStatus,
                request.GraduationYear
            });
        }

        public async Task<int> CreateShiftAsync(VolunteerShiftCreateRequest request)
        {
            const string sql = @"BEGIN PKG_VOLUNTEER_MGMT.create_shift(:VolunteerId, :PointId, :PlanStartTime, :PlanEndTime, :BackupVolunteerId, :ShiftStatus); END;";

            return await ExecuteAsync(sql, new
            {
                request.VolunteerId,
                request.PointId,
                request.BackupVolunteerId,
                request.PlanStartTime,
                request.PlanEndTime,
                request.ShiftStatus
            });
        }

        public async Task<int> CheckInShiftAsync(string shiftId, VolunteerCheckInRequest request, string operatorUserId)
        {
            const string sql = @"BEGIN PKG_VOLUNTEER_MGMT.check_in_shift(:ShiftId, :OperatorUserId, :CheckInTime, :Longitude, :Latitude, :PhotoUrl, :DistanceMeters, :CheckInStatus); END;";

            return await ExecuteAsync(sql, new
            {
                ShiftId = shiftId,
                OperatorUserId = operatorUserId,
                CheckInTime = request.CheckInTime ?? DateTime.Now,
                request.Longitude,
                request.Latitude,
                request.PhotoUrl,
                request.DistanceMeters,
                request.CheckInStatus
            });
        }

        public async Task<int> AddCreditLogAsync(VolunteerCreditLogCreateRequest request)
        {
            const string sql = @"BEGIN PKG_VOLUNTEER_MGMT.add_credit_log(:VolunteerId, :SourceType, :SourceId, :ScoreChange, :CreditLevelAfter, :CreateTime, :Remark); END;";

            return await ExecuteAsync(sql, new
            {
                request.VolunteerId,
                request.SourceType,
                request.SourceId,
                request.ScoreChange,
                request.CreditLevelAfter,
                CreateTime = request.CreateTime ?? DateTime.Now,
                request.Remark
            });
        }
    }
}
