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
                       PAYMETHOD AS PayMethod,
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
                       PAYMETHOD AS PayMethod,
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
                       PAYMETHOD AS PayMethod,
                       PAYTIME AS PayTime,
                       PUBLICFLAG AS PublicFlag
                FROM FUND_DONATIONS
                WHERE PROJECTID = :ProjectID
                ORDER BY PAYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { ProjectID = projectId });
        }

        // 财务公示保留匿名捐赠的金额，但不暴露捐赠人 ID。
        public async Task<IEnumerable<FundDonation>> GetForDisclosureByProject(string projectId)
        {
            const string sql = @"
                SELECT DONATIONID AS DonationID,
                       PROJECTID AS ProjectID,
                       CASE WHEN NVL(PUBLICFLAG, 0) = 1 THEN DONORUSERID ELSE NULL END AS DonorUserID,
                       AMOUNT AS Amount,
                       PAYMETHOD AS PayMethod,
                       PAYTIME AS PayTime,
                       NVL(PUBLICFLAG, 0) AS PublicFlag
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
                       PAYMETHOD AS PayMethod,
                       PAYTIME AS PayTime,
                       PUBLICFLAG AS PublicFlag
                FROM FUND_DONATIONS
                WHERE DONORUSERID = :DonorUserID
                ORDER BY PAYTIME DESC NULLS LAST";

            return await QueryAsync(sql, new { DonorUserID = donorUserId });
        }

        // 记录捐赠（事务）：锁定 ACTIVE 项目并累加金额，再插入捐赠记录。
        public async Task<bool> CreateWithRaisedUpdate(FundDonation donation)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                donation.DonationID = Guid.NewGuid().ToString();
                donation.PayTime ??= DateTime.Now;
                donation.PublicFlag ??= 0;

                // 通过带状态条件的 UPDATE 锁定项目，避免校验后项目已关闭仍接受捐赠。
                const string updateRaisedSql = @"
                    UPDATE FUND_CROWDFUNDINGPROJECTS
                    SET RAISEDAMOUNT = NVL(RAISEDAMOUNT, 0) + :Amount
                    WHERE PROJECTID = :ProjectID
                      AND UPPER(PROJECTSTATUS) = 'ACTIVE'";

                var updatedProjects = await ExecuteAsync(connection, transaction, updateRaisedSql, new
                {
                    Amount = donation.Amount,
                    donation.ProjectID
                });

                if (updatedProjects != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                // 插入捐赠记录
                const string insertSql = @"
                    INSERT INTO FUND_DONATIONS (DONATIONID, PROJECTID, DONORUSERID, AMOUNT,
                                                PAYMETHOD, PAYTIME, PUBLICFLAG)
                    VALUES (:DonationID, :ProjectID, :DonorUserID, :Amount,
                            :PayMethod, :PayTime, :PublicFlag)";

                await ExecuteAsync(connection, transaction, insertSql, new
                {
                    donation.DonationID,
                    donation.ProjectID,
                    donation.DonorUserID,
                    donation.Amount,
                    donation.PayMethod,
                    donation.PayTime,
                    donation.PublicFlag
                });

                transaction.Commit();
                return true;
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
