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
}

public class NamingCandidateCreateRequest
{
    public string CandidateName { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
}
