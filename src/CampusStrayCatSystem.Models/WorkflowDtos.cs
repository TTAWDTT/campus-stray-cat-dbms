namespace CampusStrayCatSystem.Models
{
    // 领养流程请求和响应模型。
    public class AdoptionApplicationCreateRequest
    {
        public string CatId { get; set; } = string.Empty;
        public string ApplicantUserId { get; set; } = string.Empty;
        public string Status { get; set; } = "PENDING";
    }

    public class AdoptionApplicationReviewRequest
    {
        public string ReviewerUserId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AgreementNo { get; set; }
        public DateTime? ConfirmTime { get; set; }
    }

    public class AdoptionVisitCreateRequest
    {
        public string VisitType { get; set; } = string.Empty;
        public DateTime? VisitTime { get; set; }
        public string VisitorUserId { get; set; } = string.Empty;
        public string? Conclusion { get; set; }
        public int PassFlag { get; set; } = 0;
    }

    public class AdoptionPendingAppDto
    {
        public string ApplicationId { get; set; } = string.Empty;
        public string CatId { get; set; } = string.Empty;
        public string? CatName { get; set; }
        public string ApplicantUserId { get; set; } = string.Empty;
        public string? ApplicantName { get; set; }
        public DateTime? ApplyTime { get; set; }
        public string? CurrentStatus { get; set; }
        public string? ReviewerUserId { get; set; }
        public string? AgreementNo { get; set; }
        public DateTime? ConfirmTime { get; set; }
    }

    public class AdoptionVisitSummaryDto
    {
        public string VisitId { get; set; } = string.Empty;
        public string ApplicationId { get; set; } = string.Empty;
        public string? CatId { get; set; }
        public string? VisitType { get; set; }
        public DateTime? VisitTime { get; set; }
        public string? VisitorUserId { get; set; }
        public string? Conclusion { get; set; }
        public int? PassFlag { get; set; }
        public string? CurrentStatus { get; set; }
    }

    // 志愿者流程请求和响应模型。
    public class VolunteerRegisterRequest
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime? JoinDate { get; set; }
        public decimal ServiceScore { get; set; } = 0;
        public string CreditLevel { get; set; } = "L1";
        public string ActiveStatus { get; set; } = "ACTIVE";
        public string? GraduationYear { get; set; }
    }

    public class VolunteerShiftCreateRequest
    {
        public string VolunteerId { get; set; } = string.Empty;
        public string PointId { get; set; } = string.Empty;
        public string? BackupVolunteerId { get; set; }
        public DateTime PlanStartTime { get; set; }
        public DateTime PlanEndTime { get; set; }
        public string ShiftStatus { get; set; } = "PLANNED";
    }

    public class VolunteerCheckInRequest
    {
        public DateTime? CheckInTime { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? DistanceMeters { get; set; }
        public string CheckInStatus { get; set; } = "CHECKED_IN";
    }

    public class VolunteerCreditLogCreateRequest
    {
        public string VolunteerId { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public decimal ScoreChange { get; set; }
        public string CreditLevelAfter { get; set; } = string.Empty;
        public DateTime? CreateTime { get; set; }
        public string? Remark { get; set; }
    }

    public class VolunteerActivityDto
    {
        public string VolunteerId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? ActiveStatus { get; set; }
        public string? CreditLevel { get; set; }
        public decimal? ServiceScore { get; set; }
        public string? ShiftId { get; set; }
        public string? ShiftStatus { get; set; }
        public DateTime? PlanStartTime { get; set; }
        public DateTime? PlanEndTime { get; set; }
    }

    public class VolunteerProfileDto
    {
        public string VolunteerId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? ActiveStatus { get; set; }
        public string? CreditLevel { get; set; }
        public decimal? ServiceScore { get; set; }
    }
}
