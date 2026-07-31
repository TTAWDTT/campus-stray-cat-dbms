using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;

namespace CampusStrayCatSystem.Data
{
    // 众筹项目数据访问实现，对应数据库表 FUND_CROWDFUNDINGPROJECTS
    public class FundCrowdfundingProjectRepository : BaseRepository<FundCrowdfundingProject>, IFundCrowdfundingProjectRepository
    {
        public FundCrowdfundingProjectRepository(IConfiguration configuration) : base(configuration) { }

        // 获取所有众筹项目，按开始时间倒序。
        public async Task<IEnumerable<FundCrowdfundingProject>> GetAll()
        {
            const string sql = @"
                SELECT PROJECTID AS ProjectID,
                       CATID AS CatID,
                       TITLE AS Title,
                       TARGETAMOUNT AS TargetAmount,
                       RAISEDAMOUNT AS RaisedAmount,
                       STARTTIME AS StartTime,
                       ENDTIME AS EndTime,
                       PROJECTSTATUS AS ProjectStatus
                FROM FUND_CROWDFUNDINGPROJECTS
                ORDER BY STARTTIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        // 按项目 ID 获取单个众筹项目。
        public async Task<FundCrowdfundingProject?> GetById(string projectId)
        {
            const string sql = @"
                SELECT PROJECTID AS ProjectID,
                       CATID AS CatID,
                       TITLE AS Title,
                       TARGETAMOUNT AS TargetAmount,
                       RAISEDAMOUNT AS RaisedAmount,
                       STARTTIME AS StartTime,
                       ENDTIME AS EndTime,
                       PROJECTSTATUS AS ProjectStatus
                FROM FUND_CROWDFUNDINGPROJECTS
                WHERE PROJECTID = :ProjectID";

            return await QuerySingleAsync(sql, new { ProjectID = projectId });
        }

        // 按状态筛选众筹项目。
        public async Task<IEnumerable<FundCrowdfundingProject>> GetByStatus(string status)
        {
            const string sql = @"
                SELECT PROJECTID AS ProjectID,
                       CATID AS CatID,
                       TITLE AS Title,
                       TARGETAMOUNT AS TargetAmount,
                       RAISEDAMOUNT AS RaisedAmount,
                       STARTTIME AS StartTime,
                       ENDTIME AS EndTime,
                       PROJECTSTATUS AS ProjectStatus
                FROM FUND_CROWDFUNDINGPROJECTS
                WHERE PROJECTSTATUS = :ProjectStatus
                ORDER BY STARTTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { ProjectStatus = status });
        }

        // 按猫咪查询众筹项目。
        public async Task<IEnumerable<FundCrowdfundingProject>> GetByCat(string catId)
        {
            const string sql = @"
                SELECT PROJECTID AS ProjectID,
                       CATID AS CatID,
                       TITLE AS Title,
                       TARGETAMOUNT AS TargetAmount,
                       RAISEDAMOUNT AS RaisedAmount,
                       STARTTIME AS StartTime,
                       ENDTIME AS EndTime,
                       PROJECTSTATUS AS ProjectStatus
                FROM FUND_CROWDFUNDINGPROJECTS
                WHERE CATID = :CatID
                ORDER BY STARTTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { CatID = catId });
        }

        // 创建众筹项目，主键使用 GUID 生成。
        public async Task<int> Create(FundCrowdfundingProject project)
        {
            // 主键使用 GUID 生成
            project.ProjectID = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO FUND_CROWDFUNDINGPROJECTS (PROJECTID, CATID, TITLE, TARGETAMOUNT,
                                                       RAISEDAMOUNT, STARTTIME, ENDTIME, PROJECTSTATUS)
                VALUES (:ProjectID, :CatID, :Title, :TargetAmount,
                        :RaisedAmount, :StartTime, :EndTime, :ProjectStatus)";

            return await ExecuteAsync(sql, new
            {
                project.ProjectID,
                project.CatID,
                project.Title,
                project.TargetAmount,
                // 新建项目已筹金额默认 0
                RaisedAmount = project.RaisedAmount ?? 0,
                project.StartTime,
                project.EndTime,
                project.ProjectStatus
            });
        }

        // 更新众筹项目基本信息。
        public async Task<int> Update(FundCrowdfundingProject project)
        {
            const string sql = @"
                UPDATE FUND_CROWDFUNDINGPROJECTS
                SET CATID = :CatID,
                    TITLE = :Title,
                    TARGETAMOUNT = :TargetAmount,
                    RAISEDAMOUNT = :RaisedAmount,
                    STARTTIME = :StartTime,
                    ENDTIME = :EndTime,
                    PROJECTSTATUS = :ProjectStatus
                WHERE PROJECTID = :ProjectID";

            return await ExecuteAsync(sql, new
            {
                project.CatID,
                project.Title,
                project.TargetAmount,
                project.RaisedAmount,
                project.StartTime,
                project.EndTime,
                project.ProjectStatus,
                project.ProjectID
            });
        }

        // 更新项目状态（如发布、结束）。
        public async Task<int> UpdateStatus(string projectId, string status)
        {
            const string sql = @"
                UPDATE FUND_CROWDFUNDINGPROJECTS
                SET PROJECTSTATUS = :ProjectStatus
                WHERE PROJECTID = :ProjectID";

            return await ExecuteAsync(sql, new
            {
                ProjectStatus = status,
                ProjectID = projectId
            });
        }

        // 判断项目是否存在。
        public async Task<bool> Exists(string projectId)
        {
            const string sql = "SELECT COUNT(1) FROM FUND_CROWDFUNDINGPROJECTS WHERE PROJECTID = :ProjectID";
            var count = await QuerySingleAsync<int>(sql, new { ProjectID = projectId });
            return count > 0;
        }
    }
}
