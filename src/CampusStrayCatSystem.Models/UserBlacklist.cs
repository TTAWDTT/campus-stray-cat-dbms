public class UserBlacklist
{
    public string BlacklistID { get; set; }      // VARCHAR2(36)
    public string UserID { get; set; }
    public string ReasonType { get; set; }       // 拉黑原因类型
    public string ReasonDetail { get; set; }
    public string ApplicationID { get; set; }    // 关联领养申请，可空
    public string CreateUserID { get; set; }
    public string? UserName { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreateTime { get; set; }
    public string BlacklistStatus { get; set; }           // ACTIVE / RELEASED
    public DateTime? ReleaseTime { get; set; }
    public string ReleasedBy { get; set; }
    public string? ReleasedByName { get; set; }
}
