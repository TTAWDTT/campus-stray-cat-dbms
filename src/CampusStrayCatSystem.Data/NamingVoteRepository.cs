using CampusStrayCatSystem.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CampusStrayCatSystem.Data;

public class NamingVoteRepository : BaseRepository<NamingCandidate>, INamingVoteRepository
{
    public NamingVoteRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<NamingCandidate>> GetCandidates(string catId)
    {
        const string sql = @"
            SELECT CANDIDATEID AS CandidateID,
                   CATID AS CatID,
                   CANDIDATENAME AS CandidateName,
                   PROPOSERUSERID AS ProposerUserID,
                   VOTECOUNT AS VoteCount,
                   DEADLINE AS Deadline,
                   WINFLAG AS WinFlag
            FROM VOTE_NAMINGCANDIDATES
            WHERE CATID = :CatID
            ORDER BY WINFLAG DESC, VOTECOUNT DESC, CANDIDATEID";

        return await QueryAsync(sql, new { CatID = catId });
    }

    public async Task<int> CreateCandidate(NamingCandidate candidate)
    {
        const string sql = @"
            INSERT INTO VOTE_NAMINGCANDIDATES
                (CANDIDATEID, CATID, CANDIDATENAME, PROPOSERUSERID, VOTECOUNT, DEADLINE, WINFLAG)
            VALUES
                (:CandidateID, :CatID, :CandidateName, :ProposerUserID, 0, :Deadline, 0)";

        return await ExecuteAsync(sql, candidate);
    }

    public async Task<bool> Vote(string candidateId, string voterUserId)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string candidateSql = @"
                SELECT CATID AS CatID, DEADLINE AS Deadline, WINFLAG AS WinFlag
                FROM VOTE_NAMINGCANDIDATES
                WHERE CANDIDATEID = :CandidateID";
            var candidate = await connection.QuerySingleOrDefaultAsync<NamingCandidate>(candidateSql,
                new { CandidateID = candidateId }, transaction);
            if (candidate == null || string.IsNullOrWhiteSpace(candidate.CatID))
            {
                transaction.Rollback();
                return false;
            }

            var archiveStatus = await connection.ExecuteScalarAsync<string?>(@"
                SELECT ARCHIVESTATUS FROM CAT_CATS
                WHERE CATID = :CatID
                FOR UPDATE", new { CatID = candidate.CatID }, transaction);
            if (CatStatusCodes.NormalizeArchiveStatus(archiveStatus) == CatStatusCodes.ArchiveArchived)
            {
                transaction.Rollback();
                return false;
            }

            candidate = await connection.QuerySingleOrDefaultAsync<NamingCandidate>(@"
                SELECT CATID AS CatID, DEADLINE AS Deadline, WINFLAG AS WinFlag
                FROM VOTE_NAMINGCANDIDATES
                WHERE CANDIDATEID = :CandidateID
                FOR UPDATE", new { CandidateID = candidateId }, transaction);
            if (candidate == null || candidate.WinFlag == 1 ||
                (candidate.Deadline.HasValue && candidate.Deadline.Value <= DateTime.Now))
            {
                transaction.Rollback();
                return false;
            }

            try
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO VOTE_NAMINGRECORDS
                        (RECORDID, CANDIDATEID, VOTERUSERID, VOTETIME, VOTESTATUS)
                    VALUES (:RecordID, :CandidateID, :VoterUserID, SYSDATE, 'VALID')",
                    new { RecordID = Guid.NewGuid().ToString(), CandidateID = candidateId, VoterUserID = voterUserId }, transaction);
            }
            catch (OracleException ex) when (ex.Number == 1)
            {
                transaction.Rollback();
                return false;
            }

            var rows = await connection.ExecuteAsync(@"
                UPDATE VOTE_NAMINGCANDIDATES
                SET VOTECOUNT = NVL(VOTECOUNT, 0) + 1
                WHERE CANDIDATEID = :CandidateID",
                new { CandidateID = candidateId }, transaction);
            if (rows != 1)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> SelectWinner(string candidateId)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var target = await connection.QuerySingleOrDefaultAsync<NamingCandidate>(@"
                SELECT CANDIDATEID AS CandidateID,
                       CATID AS CatID,
                       CANDIDATENAME AS CandidateName,
                       VOTECOUNT AS VoteCount,
                       DEADLINE AS Deadline,
                       WINFLAG AS WinFlag
                FROM VOTE_NAMINGCANDIDATES
                WHERE CANDIDATEID = :CandidateID",
                new { CandidateID = candidateId }, transaction);
            if (target == null || string.IsNullOrWhiteSpace(target.CatID))
            {
                transaction.Rollback();
                return false;
            }

            var archiveStatus = await connection.ExecuteScalarAsync<string?>(@"
                SELECT ARCHIVESTATUS
                FROM CAT_CATS
                WHERE CATID = :CatID
                FOR UPDATE", new { CatID = target.CatID }, transaction);
            if (CatStatusCodes.NormalizeArchiveStatus(archiveStatus) == CatStatusCodes.ArchiveArchived ||
                (target.Deadline.HasValue && target.Deadline.Value > DateTime.Now))
            {
                transaction.Rollback();
                return false;
            }

            var candidates = (await connection.QueryAsync<NamingCandidate>(@"
                SELECT CANDIDATEID AS CandidateID,
                       NVL(VOTECOUNT, 0) AS VoteCount,
                       WINFLAG AS WinFlag
                FROM VOTE_NAMINGCANDIDATES
                WHERE CATID = :CatID
                FOR UPDATE", new { CatID = target.CatID }, transaction)).ToList();
            var highest = candidates.Max(x => x.VoteCount);
            var leaders = candidates.Where(x => x.VoteCount == highest).ToList();
            if (target.WinFlag == 1 || leaders.Count != 1 || leaders[0].CandidateID != target.CandidateID)
            {
                transaction.Rollback();
                return false;
            }

            await connection.ExecuteAsync(
                "UPDATE VOTE_NAMINGCANDIDATES SET WINFLAG = 0 WHERE CATID = :CatID",
                new { CatID = target.CatID }, transaction);
            var rows = await connection.ExecuteAsync(@"
                UPDATE VOTE_NAMINGCANDIDATES
                SET WINFLAG = 1
                WHERE CANDIDATEID = :CandidateID",
                new { CandidateID = candidateId }, transaction);
            if (rows != 1)
            {
                transaction.Rollback();
                return false;
            }

            await connection.ExecuteAsync(@"
                UPDATE CAT_CATS
                SET CATNAME = (SELECT CANDIDATENAME FROM VOTE_NAMINGCANDIDATES WHERE CANDIDATEID = :CandidateID)
                WHERE CATID = :CatID",
                new { CandidateID = candidateId, CatID = target.CatID }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
