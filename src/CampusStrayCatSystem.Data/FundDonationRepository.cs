using Microsoft.Extensions.Configuration;
using CampusStrayCatSystem.Models;
using System.Data;

namespace CampusStrayCatSystem.Data
{
    // 捐赠数据访问实现，对应数据库表 FUND_DONATIONS
    public class FundDonationRepository : BaseRepository<FundDonation>, IFundDonationRepository
    {
        public FundDonationRepository(IConfiguration configuration) : base(configuration) { }

        // 获取所有捐赠记录，按支付时间倒序。
        public async Task<IEnumerable<FundDonation>> GetAll()
        {
            const string sql = @"
                SELECT DONATIONID AS DonationID,
                       PROJECTID AS ProjectID,
                       DONORUSERID AS DonorUserID,
                       AMOUNT AS Amount,
                       PAYTIME AS PayTime,
                       PUBLICFLAG AS PublicFlag
                FROM FUND_DONATIONS
                ORDER BY PAYTIME DESC NULLS LAST";

            return await QueryAsync(sql);
        }

        // 按捐赠 ID 获取单条捐赠记录。
        public async Task<FundDonation?> GetById(string donationId)
        {
            const string sql = @"
                SELECT DONATIONID AS DonationID,
                       PROJECTID AS ProjectID,
                       DONORUSERID AS DonorUserID,
                       AMOUNT AS Amount,
                       PAYTIME AS PayTime,
                       PUBLICFLAG AS PublicFlag
                FROM FUND_DONATIONS
                WHERE DONATIONID = :DonationID";

            return await QuerySingleAsync(sql, new { DonationID = donationId });
        }

        // 按项目查询捐赠记录。
        public async Task<IEnumerable<FundDonation>> GetByProject(string projectId)
        {
            const string sql = @"
                SELECT DONATIONID AS DonationID,
                       PROJECTID AS ProjectID,
                       DONORUSERID AS DonorUserID,
                       AMOUNT AS Amount,
                       PAYTIME AS PayTime,
                       PUBLICFLAG AS PublicFlag
                FROM FUND_DONATIONS
                WHERE PROJECTID = :ProjectID
                ORDER BY PAYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { ProjectID = projectId });
        }

        // 按捐赠人查询其捐赠记录。
        public async Task<IEnumerable<FundDonation>> GetByDonor(string donorUserId)
        {
            const string sql = @"
                SELECT DONATIONID AS DonationID,
                       PROJECTID AS ProjectID,
                       DONORUSERID AS DonorUserID,
                       AMOUNT AS Amount,
                       PAYTIME AS PayTime,
                       PUBLICFLAG AS PublicFlag
                FROM FUND_DONATIONS
                WHERE DONORUSERID = :DonorUserID
                ORDER BY PAYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { DonorUserID = donorUserId });
        }

        // 记录捐赠（事务）：1) 插入捐赠记录；2) 累加项目已筹金额。
        public async Task CreateWithRaisedUpdate(FundDonation donation)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                donation.DonationID = Guid.NewGuid().ToString();

                // 插入捐赠记录
                const string insertSql = @"
                    INSERT INTO FUND_DONATIONS (DONATIONID, PROJECTID, DONORUSERID, AMOUNT,
                                                PAYTIME, PUBLICFLAG)
                    VALUES (:DonationID, :ProjectID, :DonorUserID, :Amount,
                            :PayTime, :PublicFlag)";

                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    donation.DonationID,
                    donation.ProjectID,
                    donation.DonorUserID,
                    donation.Amount,
                    // 若未显式传入支付时间，则默认当前时间
                    PayTime = donation.PayTime ?? DateTime.Now,
                    // 默认公开（1）
                    PublicFlag = donation.PublicFlag ?? 1
                });

                // 累加项目已筹金额（使用 RAISEDAMOUNT = RAISEDAMOUNT + :Amount）
                const string updateRaisedSql = @"
                    UPDATE FUND_CROWDFUNDINGPROJECTS
                    SET RAISEDAMOUNT = NVL(RAISEDAMOUNT, 0) + :Amount
                    WHERE PROJECTID = :ProjectID";

                await ExecuteAsync(connection, transaction, updateRaisedSql, new
                {
                    Amount = donation.Amount ?? 0,
                    donation.ProjectID
                });

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // 统计某项目的捐赠总金额。
        public async Task<decimal> GetTotalDonationByProject(string projectId)
        {
            // 汇总某项目的所有捐赠金额
            const string sql = @"
                SELECT NVL(SUM(AMOUNT), 0)
                FROM FUND_DONATIONS
                WHERE PROJECTID = :ProjectID";

            var total = await QuerySingleAsync<decimal>(sql, new { ProjectID = projectId });
            return total;
        }

        // 统计某项目的捐赠笔数。
        public async Task<int> GetDonationCountByProject(string projectId)
        {
            // 统计某项目的捐赠笔数
            const string sql = @"
                SELECT COUNT(1)
                FROM FUND_DONATIONS
                WHERE PROJECTID = :ProjectID";

            var count = await QuerySingleAsync<int>(sql, new { ProjectID = projectId });
            return count;
        }
    }
}
