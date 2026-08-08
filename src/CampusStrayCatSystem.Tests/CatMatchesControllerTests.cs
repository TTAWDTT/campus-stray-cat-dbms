using System.Security.Claims;
using CampusStrayCatSystem.Core;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Tests;

public class CatMatchesControllerTests
{
    [Fact]
    public async Task GetMatchesRejectsUnsafeIdentifier()
    {
        var matches = new FakeCatMatchRepository();
        var controller = CreateController(matches);

        var response = await controller.GetMatches("../cat", "photo-1");

        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(0, matches.GetBySourcePhotoCalls);
    }

    [Fact]
    public async Task GetMatchesRejectsUnknownStatus()
    {
        var matches = new FakeCatMatchRepository();
        var controller = CreateController(matches);

        var response = await controller.GetMatches("cat-1", "photo-1", confirmStatus: "DONE");

        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(0, matches.GetBySourcePhotoCalls);
    }

    [Fact]
    public async Task GetMatchesReturnsNotFoundWhenPhotoDoesNotBelongToCat()
    {
        var matches = new FakeCatMatchRepository();
        var photos = new FakeCatPhotoRepository();
        var controller = CreateController(matches, photos);

        var response = await controller.GetMatches("cat-1", "photo-1");

        Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal(0, matches.GetBySourcePhotoCalls);
    }

    [Fact]
    public async Task GetMatchesNormalizesFiltersAndReturnsRecords()
    {
        var matches = new FakeCatMatchRepository {
            Matches = [new CatMatchRecord {
                MatchID = "match-1",
                SourcePhotoID = "photo-1",
                CandidateCatID = "candidate-1",
                ConfirmStatus = CatMatchStatusCodes.Pending
            }]
        };
        var photos = new FakeCatPhotoRepository {
            Photo = new CatPhoto { CatID = "cat-1", PhotoID = "photo-1" }
        };
        var controller = CreateController(matches, photos);

        var response = await controller.GetMatches("cat-1", "photo-1", "candidate-1", " confirmed ");

        var result = Assert.IsType<OkObjectResult>(response.Result);
        var records = Assert.IsAssignableFrom<IEnumerable<CatMatchRecord>>(result.Value);
        Assert.Single(records);
        Assert.Equal("candidate-1", matches.LastCandidateCatID);
        Assert.Equal(CatMatchStatusCodes.Confirmed, matches.LastConfirmStatus);
    }

    [Fact]
    public async Task GetMatchRejectsUnsafeIdentifier()
    {
        var matches = new FakeCatMatchRepository();
        var controller = CreateController(matches);

        var response = await controller.GetMatch("../match");

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetMatchReturnsNotFoundWhenRecordDoesNotExist()
    {
        var controller = CreateController(new FakeCatMatchRepository());

        var response = await controller.GetMatch("missing-match");

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetMatchReturnsRecord()
    {
        var expected = new CatMatchRecord { MatchID = "match-1" };
        var controller = CreateController(new FakeCatMatchRepository { Record = expected });

        var response = await controller.GetMatch("match-1");

        var result = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public async Task ConfirmMatchRejectsPendingStatus()
    {
        var matches = new FakeCatMatchRepository();
        var controller = CreateController(matches, userID: "user-1");

        var response = await controller.ConfirmMatch("match-1", new ConfirmCatMatchRequest {
            ConfirmStatus = CatMatchStatusCodes.Pending
        });

        Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(0, matches.ConfirmCalls);
    }

    [Fact]
    public async Task ConfirmMatchReturnsUnauthorizedWithoutUserClaim()
    {
        var matches = new FakeCatMatchRepository();
        var controller = CreateController(matches);

        var response = await controller.ConfirmMatch("match-1", new ConfirmCatMatchRequest {
            ConfirmStatus = CatMatchStatusCodes.Confirmed
        });

        Assert.IsType<UnauthorizedObjectResult>(response);
        Assert.Equal(0, matches.ConfirmCalls);
    }

    [Fact]
    public async Task ConfirmMatchMapsRepositoryNotFound()
    {
        var matches = new FakeCatMatchRepository {
            ConfirmResult = CatMatchMutationStatus.MatchNotFound
        };
        var controller = CreateController(matches, userID: "user-1");

        var response = await controller.ConfirmMatch("match-1", new ConfirmCatMatchRequest {
            ConfirmStatus = "confirmed"
        });

        Assert.IsType<NotFoundObjectResult>(response);
    }

    [Fact]
    public async Task ConfirmMatchMapsUnavailableAssociationToConflict()
    {
        var matches = new FakeCatMatchRepository {
            ConfirmResult = CatMatchMutationStatus.AssociationUnavailable
        };
        var controller = CreateController(matches, userID: "user-1");

        var response = await controller.ConfirmMatch("match-1", new ConfirmCatMatchRequest {
            ConfirmStatus = CatMatchStatusCodes.Rejected
        });

        Assert.IsType<ConflictObjectResult>(response);
    }

    [Fact]
    public async Task ConfirmMatchPassesNormalizedStatusAndCurrentUser()
    {
        var matches = new FakeCatMatchRepository();
        var controller = CreateController(matches, userID: "user-1");

        var response = await controller.ConfirmMatch("match-1", new ConfirmCatMatchRequest {
            ConfirmStatus = " rejected "
        });

        Assert.IsType<NoContentResult>(response);
        Assert.Equal(1, matches.ConfirmCalls);
        Assert.Equal("match-1", matches.LastMatchID);
        Assert.Equal(CatMatchStatusCodes.Rejected, matches.LastConfirmStatus);
        Assert.Equal("user-1", matches.LastConfirmUserID);
    }

    private static CatMatchesController CreateController(FakeCatMatchRepository matches,
                                                          FakeCatPhotoRepository? photos = null,
                                                          string? userID = null)
    {
        var controller = new CatMatchesController(matches, photos ?? new FakeCatPhotoRepository());
        var identity = userID == null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userID)], "test");
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }
}

internal sealed class FakeCatMatchRepository : ICatMatchRepository
{
    public IEnumerable<CatMatchRecord> Matches { get; set; } = [];
    public CatMatchRecord? Record { get; set; }
    public CatMatchMutationStatus ConfirmResult { get; set; } = CatMatchMutationStatus.Success;
    public int GetBySourcePhotoCalls { get; private set; }
    public int ConfirmCalls { get; private set; }
    public string? LastCandidateCatID { get; private set; }
    public string? LastConfirmStatus { get; private set; }
    public string? LastMatchID { get; private set; }
    public string? LastConfirmUserID { get; private set; }

    public Task<IEnumerable<CatMatchRecord>> GetBySourcePhotoAsync(string catID,
                                                                    string photoID,
                                                                    string? candidateCatID,
                                                                    string? confirmStatus)
    {
        GetBySourcePhotoCalls++;
        LastCandidateCatID = candidateCatID;
        LastConfirmStatus = confirmStatus;
        return Task.FromResult(Matches);
    }

    public Task<CatMatchRecord?> GetByIDAsync(string matchID) => Task.FromResult(Record);

    public Task<CatMatchMutationStatus> ConfirmAsync(string matchID,
                                                     string confirmStatus,
                                                     string confirmUserID)
    {
        ConfirmCalls++;
        LastMatchID = matchID;
        LastConfirmStatus = confirmStatus;
        LastConfirmUserID = confirmUserID;
        return Task.FromResult(ConfirmResult);
    }
}
