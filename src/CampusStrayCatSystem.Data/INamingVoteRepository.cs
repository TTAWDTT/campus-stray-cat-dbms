using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data;

public interface INamingVoteRepository
{
    Task<IEnumerable<NamingCandidate>> GetCandidates(string catId);
    Task<int> CreateCandidate(NamingCandidate candidate);
    Task<bool> Vote(string candidateId, string voterUserId);
    Task<bool> SelectWinner(string candidateId);
}
