namespace CampusStrayCatSystem.Models;

public class NamingCandidate
{
    public string CandidateID { get; set; } = string.Empty;
    public string CatID { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string? ProposerUserID { get; set; }
    public int VoteCount { get; set; }
    public DateTime? Deadline { get; set; }
    public int WinFlag { get; set; }
    public string? ArchiveStatus { get; set; }
}

public class NamingCandidateCreateRequest
{
    [Utf8ByteLength(50, ErrorMessage = "候选名称不能超过数据库允许的 50 字节。")]
    public string CandidateName { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
}
