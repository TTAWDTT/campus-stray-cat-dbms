using System.Security.Claims;
using System.Text;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Core;

[Route("api/naming-votes")]
[ApiController]
[Authorize]
public class NamingVotesController : ControllerBase
{
    private readonly INamingVoteRepository _repository;
    private readonly ICatRepository _catRepository;

    public NamingVotesController(INamingVoteRepository repository, ICatRepository catRepository)
    {
        _repository = repository;
        _catRepository = catRepository;
    }

    [HttpGet("cats/{catId}/candidates")]
    public async Task<ActionResult<IEnumerable<NamingCandidate>>> GetCandidates(string catId)
    {
        if (string.IsNullOrWhiteSpace(catId)) return BadRequest("猫咪 ID 不能为空。");
        if (!await _catRepository.Exists(catId)) return NotFound("猫咪不存在。");
        return Ok(await _repository.GetCandidates(catId));
    }

    [HttpPost("cats/{catId}/candidates")]
    [Authorize(Roles = "ADMIN,VOLUNTEER")]
    public async Task<ActionResult<NamingCandidate>> CreateCandidate(
        string catId, [FromBody] NamingCandidateCreateRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CandidateName))
            return BadRequest("候选名称不能为空。");
        var candidateName = request.CandidateName.Trim();
        if (Encoding.UTF8.GetByteCount(candidateName) > 50)
            return BadRequest("候选名称不能超过 50 个字节。");
        if (request.Deadline.HasValue && request.Deadline.Value <= DateTime.Now)
            return BadRequest("投票截止时间必须晚于当前时间。");
        var cat = await _catRepository.GetByIdAsync(catId);
        if (cat == null) return NotFound("猫咪不存在。");
        if (CatStatusCodes.NormalizeArchiveStatus(cat.ArchiveStatus) == CatStatusCodes.ArchiveArchived)
            return Conflict("已归档的猫咪不能发起命名投票。");

        var proposer = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(proposer)) return Unauthorized();
        var candidate = new NamingCandidate
        {
            CandidateID = Guid.NewGuid().ToString(),
            CatID = catId,
            CandidateName = candidateName,
            ProposerUserID = proposer,
            Deadline = request.Deadline
        };
        if (await _repository.CreateCandidate(candidate) != 1)
            return Conflict("候选名称创建未生效。");
        return CreatedAtAction(nameof(GetCandidates), new { catId }, candidate);
    }

    [HttpPost("candidates/{candidateId}/vote")]
    public async Task<IActionResult> Vote(string candidateId)
    {
        var voter = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(voter)) return Unauthorized();
        return await _repository.Vote(candidateId, voter)
            ? Ok(new { message = "投票成功。" })
            : Conflict("候选名称不存在、猫咪已归档、投票已截止、已有获胜名称或该用户已经投过票。");
    }

    [HttpPost("candidates/{candidateId}/winner")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> SelectWinner(string candidateId)
    {
        return await _repository.SelectWinner(candidateId)
            ? Ok(new { message = "已确定获胜名称。" })
            : Conflict("候选不存在、投票尚未截止、猫咪已归档或票数并列，无法确定获胜名称。");
    }
}
