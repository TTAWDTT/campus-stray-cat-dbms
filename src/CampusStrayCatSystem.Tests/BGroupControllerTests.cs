using System.Security.Claims;
using CampusStrayCatSystem.Core;
using CampusStrayCatSystem.Data;
using CampusStrayCatSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusStrayCatSystem.Tests;

public class CatsControllerTests
{
    [Fact]
    public async Task GetCatsRejectsUnknownLifeStatus()
    {
        var repository = new FakeCatCatalogRepository();
        var controller = new CatsController(repository, new FakeCampusAreaRepository());

        var response = await controller.GetCats(lifeStatus: "UNKNOWN_STATUS");

        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(0, repository.GetAllCalls);
    }

    [Fact]
    public async Task GetCatsNormalizesStatusFilters()
    {
        var repository = new FakeCatCatalogRepository {
            Cats = [new CatSummary { CatID = "cat-1" }]
        };
        var controller = new CatsController(repository, new FakeCampusAreaRepository());

        var response = await controller.GetCats(" area-1 ", "在校", "正常");

        Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal("area-1", repository.LastMainAreaID);
        Assert.Equal(CatStatusCodes.LifeOnCampus, repository.LastLifeStatus);
        Assert.Equal(CatStatusCodes.ArchivePublished, repository.LastArchiveStatus);
    }

    [Fact]
    public async Task CreateCatRejectsUnknownArea()
    {
        var areas = new FakeCampusAreaRepository();
        var controller = new CatsController(new FakeCatCatalogRepository(), areas);

        var response = await controller.CreateCat(new CreateCatRequest {
            CatName = "Test cat",
            MainAreaId = "missing-area"
        });

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }
}

public class NamingVotesControllerTests
{
    [Fact]
    public async Task GetCandidatesReturnsNotFoundWhenCatDoesNotExist()
    {
        var cats = new FakeCatCatalogRepository { ExistsResult = false };
        var controller = new NamingVotesController(new FakeNamingVoteRepository(), cats);

        var response = await controller.GetCandidates("missing-cat");

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateCandidateRejectsArchivedCat()
    {
        var cats = new FakeCatCatalogRepository {
            Cat = new CatSummary {
                CatID = "cat-1",
                ArchiveStatus = CatStatusCodes.ArchiveArchived
            }
        };
        var controller = CreateNamingController(new FakeNamingVoteRepository(), cats, "user-1");

        var response = await controller.CreateCandidate("cat-1", new NamingCandidateCreateRequest {
            CandidateName = "小花"
        });

        Assert.IsType<ConflictObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateCandidateUsesCurrentUserAndTrimmedName()
    {
        var votes = new FakeNamingVoteRepository();
        var cats = new FakeCatCatalogRepository {
            Cat = new CatSummary {
                CatID = "cat-1",
                ArchiveStatus = CatStatusCodes.ArchivePublished
            }
        };
        var controller = CreateNamingController(votes, cats, "user-1");

        var response = await controller.CreateCandidate("cat-1", new NamingCandidateCreateRequest {
            CandidateName = " 小花 "
        });

        Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.NotNull(votes.CreatedCandidate);
        Assert.Equal("小花", votes.CreatedCandidate!.CandidateName);
        Assert.Equal("user-1", votes.CreatedCandidate.ProposerUserID);
    }

    [Fact]
    public async Task VoteReturnsConflictWhenRepositoryRejectsVote()
    {
        var votes = new FakeNamingVoteRepository { VoteResult = false };
        var controller = CreateNamingController(votes, new FakeCatCatalogRepository(), "user-1");

        var response = await controller.Vote("candidate-1");

        Assert.IsType<ConflictObjectResult>(response);
        Assert.Equal("user-1", votes.LastVoterUserID);
    }

    private static NamingVotesController CreateNamingController(FakeNamingVoteRepository votes,
                                                                 FakeCatCatalogRepository cats,
                                                                 string userID)
    {
        var controller = new NamingVotesController(votes, cats);
        SetUser(controller, userID);
        return controller;
    }

    private static void SetUser(ControllerBase controller, string userID)
    {
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userID)
                ], "test"))
            }
        };
    }
}

public class CampusAreasAndServicePointsControllerTests
{
    [Fact]
    public async Task CreateAreaRejectsEmptyName()
    {
        var controller = new CampusAreasController(new FakeCampusAreaRepository());

        var response = await controller.CreateArea(new CampusArea());

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task UpdateAreaRejectsParentCycle()
    {
        var areas = new FakeCampusAreaRepository {
            Areas = new Dictionary<string, CampusArea> {
                ["area-1"] = new CampusArea { AreaID = "area-1", AreaName = "A", ParentAreaID = "area-2" },
                ["area-2"] = new CampusArea { AreaID = "area-2", AreaName = "B", ParentAreaID = "area-1" }
            }
        };
        var controller = new CampusAreasController(areas);

        var response = await controller.UpdateArea("area-1", new CampusArea {
            AreaID = "area-1",
            AreaName = "A",
            ParentAreaID = "area-2"
        });

        Assert.IsType<BadRequestObjectResult>(response);
    }

    [Fact]
    public async Task DeleteAreaReturnsConflictWhenChildrenExist()
    {
        var areas = new FakeCampusAreaRepository {
            Children = [new CampusArea { AreaID = "child-1", ParentAreaID = "area-1" }]
        };
        var controller = new CampusAreasController(areas);

        var response = await controller.DeleteArea("area-1");

        Assert.IsType<ConflictObjectResult>(response);
    }

    [Fact]
    public async Task CreateServicePointRejectsUnpairedCoordinates()
    {
        var controller = new ServicePointsController(
            new FakeServicePointRepository(),
            new FakeCampusAreaRepository());

        var response = await controller.CreatePoint(new ServicePoint {
            PointName = "point-1",
            Longitude = 121.5m
        });

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }
}

public class CatSightingsAndNestMaintenanceControllerTests
{
    [Fact]
    public async Task GetSightingsRejectsReversedTimeRange()
    {
        var controller = new CatSightingsController(
            new FakeCatSightingRepository(),
            new FakeCampusAreaRepository());

        var response = await controller.GetSightings(
            null,
            null,
            new DateTime(2026, 8, 2),
            new DateTime(2026, 8, 1));

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetRecentSightingsRejectsOutOfRangeLimit()
    {
        var controller = new CatSightingsController(
            new FakeCatSightingRepository(),
            new FakeCampusAreaRepository());

        var response = await controller.GetRecentByCat("cat-1", 101);

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateSightingRejectsInvalidCoordinates()
    {
        var controller = new CatSightingsController(
            new FakeCatSightingRepository(),
            new FakeCampusAreaRepository());
        SetUser(controller, "user-1");

        var response = await controller.CreateSighting(new CatSighting {
            CatID = "cat-1",
            AreaID = "area-1",
            Longitude = 181m,
            Latitude = 31m
        });

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateMaintenanceRecordRejectsNextCheckBeforeCheckTime()
    {
        var controller = new NestMaintenanceRecordsController(
            new FakeNestMaintenanceRepository(),
            new FakeServicePointRepository {
                Point = new ServicePoint { PointID = "point-1", PointName = "point-1" }
            });
        SetUser(controller, "user-1");

        var response = await controller.CreateRecord(new NestMaintenanceRecord {
            PointID = "point-1",
            ActionType = "清理",
            CheckTime = new DateTime(2026, 8, 2),
            NextCheckTime = new DateTime(2026, 8, 1)
        });

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    private static void SetUser(ControllerBase controller, string userID)
    {
        controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userID)
                ], "test"))
            }
        };
    }
}

internal sealed class FakeCatCatalogRepository : ICatRepository
{
    public bool ExistsResult { get; set; } = true;
    public CatSummary? Cat { get; set; }
    public IEnumerable<CatSummary> Cats { get; set; } = [];
    public int GetAllCalls { get; private set; }
    public string? LastMainAreaID { get; private set; }
    public string? LastLifeStatus { get; private set; }
    public string? LastArchiveStatus { get; private set; }

    public Task<bool> Exists(string catId) => Task.FromResult(ExistsResult);

    public Task<IEnumerable<CatSummary>> GetAllAsync(string? mainAreaId = null,
                                                      string? lifeStatus = null,
                                                      string? archiveStatus = null)
    {
        GetAllCalls++;
        LastMainAreaID = mainAreaId;
        LastLifeStatus = lifeStatus;
        LastArchiveStatus = archiveStatus;
        return Task.FromResult(Cats);
    }

    public Task<CatSummary?> GetByIdAsync(string catId) => Task.FromResult(Cat);
    public Task<CatSummary?> CreateAsync(Cat cat) => Task.FromResult<CatSummary?>(new CatSummary {
        CatID = cat.CatID,
        CatName = cat.CatName,
        ArchiveStatus = cat.ArchiveStatus
    });
    public Task<int> UpdateAsync(Cat cat) => Task.FromResult(1);
    public Task<int> ArchiveAsync(string catId) => Task.FromResult(1);
}

internal sealed class FakeCampusAreaRepository : ICampusAreaRepository
{
    public Dictionary<string, CampusArea> Areas { get; set; } = [];
    public IEnumerable<CampusArea> Children { get; set; } = [];
    public bool HasReferencesResult { get; set; }

    public Task<IEnumerable<CampusArea>> GetAllAsync(string? campusName = null,
                                                      string? areaType = null,
                                                      string? riskLevel = null) =>
        Task.FromResult<IEnumerable<CampusArea>>(Areas.Values);
    public Task<CampusArea?> GetByIdAsync(string id) =>
        Task.FromResult(Areas.TryGetValue(id, out var area) ? area :
            id == "area-1" ? new CampusArea { AreaID = id, AreaName = "A" } : null);
    public Task<IEnumerable<CampusArea>> GetRootsAsync() => Task.FromResult<IEnumerable<CampusArea>>([]);
    public Task<IEnumerable<CampusArea>> GetChildrenAsync(string parentAreaId) =>
        Task.FromResult(Children.Where(area => area.ParentAreaID == parentAreaId));
    public Task<IEnumerable<CampusAreaHierarchyItem>> GetHierarchyAsync() =>
        Task.FromResult<IEnumerable<CampusAreaHierarchyItem>>([]);
    public Task<bool> HasReferencesAsync(string id) => Task.FromResult(HasReferencesResult);
    public Task<int> CreateAsync(CampusArea area) => Task.FromResult(1);
    public Task<int> UpdateAsync(CampusArea area) => Task.FromResult(1);
    public Task<int> DeleteAsync(string id) => Task.FromResult(1);
}

internal sealed class FakeNamingVoteRepository : INamingVoteRepository
{
    public NamingCandidate? CreatedCandidate { get; private set; }
    public bool VoteResult { get; set; } = true;
    public string? LastVoterUserID { get; private set; }

    public Task<IEnumerable<NamingCandidate>> GetCandidates(string catId) =>
        Task.FromResult<IEnumerable<NamingCandidate>>([]);
    public Task<int> CreateCandidate(NamingCandidate candidate)
    {
        CreatedCandidate = candidate;
        return Task.FromResult(1);
    }
    public Task<bool> Vote(string candidateId, string voterUserId)
    {
        LastVoterUserID = voterUserId;
        return Task.FromResult(VoteResult);
    }
    public Task<bool> SelectWinner(string candidateId) => Task.FromResult(true);
}

internal sealed class FakeCatSightingRepository : ICatSightingRepository
{
    public bool CatExistsResult { get; set; } = true;
    public bool UserExistsResult { get; set; } = true;

    public Task<IEnumerable<CatSighting>> GetAllAsync(string? catId = null,
                                                       string? areaId = null,
                                                       DateTime? from = null,
                                                       DateTime? to = null) =>
        Task.FromResult<IEnumerable<CatSighting>>([]);
    public Task<CatSighting?> GetByIdAsync(string id) => Task.FromResult<CatSighting?>(null);
    public Task<IEnumerable<CatSighting>> GetRecentByCatAsync(string catId, int limit) =>
        Task.FromResult<IEnumerable<CatSighting>>([]);
    public Task<bool> CatExistsAsync(string catId) => Task.FromResult(CatExistsResult);
    public Task<bool> UserExistsAsync(string userId) => Task.FromResult(UserExistsResult);
    public Task<bool> HasReferencesAsync(string id) => Task.FromResult(false);
    public Task<int> CreateAsync(CatSighting sighting) => Task.FromResult(1);
    public Task<int> UpdateAsync(CatSighting sighting) => Task.FromResult(1);
    public Task<int> DeleteAsync(string id) => Task.FromResult(1);
}

internal sealed class FakeServicePointRepository : IServicePointRepository
{
    public ServicePoint? Point { get; set; }
    public bool HasReferencesResult { get; set; }

    public Task<IEnumerable<ServicePoint>> GetAllAsync(string? areaId = null,
                                                        string? pointType = null,
                                                        string? facilityStatus = null) =>
        Task.FromResult<IEnumerable<ServicePoint>>([]);
    public Task<ServicePoint?> GetByIdAsync(string id) => Task.FromResult(Point);
    public Task<bool> HasReferencesAsync(string id) => Task.FromResult(HasReferencesResult);
    public Task<int> CreateAsync(ServicePoint point) => Task.FromResult(1);
    public Task<int> UpdateAsync(ServicePoint point) => Task.FromResult(1);
    public Task<int> DeleteAsync(string id) => Task.FromResult(1);
}

internal sealed class FakeNestMaintenanceRepository : INestMaintenanceRecordRepository
{
    public Task<IEnumerable<NestMaintenanceRecord>> GetAllAsync(string? pointId = null,
                                                                 string? damageLevel = null,
                                                                 DateTime? from = null,
                                                                 DateTime? to = null) =>
        Task.FromResult<IEnumerable<NestMaintenanceRecord>>([]);
    public Task<NestMaintenanceRecord?> GetByIdAsync(string id) =>
        Task.FromResult<NestMaintenanceRecord?>(null);
    public Task<bool> UserExistsAsync(string userId) => Task.FromResult(true);
    public Task<int> CreateAsync(NestMaintenanceRecord record) => Task.FromResult(1);
    public Task<int> UpdateAsync(NestMaintenanceRecord record) => Task.FromResult(1);
    public Task<int> DeleteAsync(string id) => Task.FromResult(1);
}
