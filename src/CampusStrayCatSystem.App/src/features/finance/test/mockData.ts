/**
 * 财务管理模块 — 模拟数据
 *
 * 数据完全按照 finance.service.ts 中导出的接口定义构造：
 *   CrowdfundingProject / ExpenseRecord / DonationRecord / FinancialDisclosureSummary
 *
 * 将 USE_MOCK 设为 true 即可使用此数据预览，设为 false 则调用真实 API。
 */

/** 财务管理模块 Mock 开关 — 统一设为 true 或 false 即可切换所有页面的数据来源 */
export const USE_MOCK = false

import type {
    CrowdfundingProject,
    ExpenseRecord,
    DonationRecord,
    FinancialDisclosureSummary,
    FinancialDisclosureDetail,
    SnapshotRecord,
} from '../../../services/finance.service'

// ═══════════════════════════════════════════
// 1. 众筹项目
// ═══════════════════════════════════════════
export const mockProjects: CrowdfundingProject[] = [
    {
        ProjectID: 'PRJ-001',
        CatID: 'CAT-001',
        Title: '橘座的口炎治疗基金',
        TargetAmount: 5000,
        RaisedAmount: 3850,
        StartTime: '2026-01-15T00:00:00Z',
        EndTime: '2026-12-31T00:00:00Z',
        ProjectStatus: 'ACTIVE',
    },
    {
        ProjectID: 'PRJ-002',
        CatID: 'CAT-002',
        Title: '小白猫三联疫苗众筹',
        TargetAmount: 2000,
        RaisedAmount: 2000,
        StartTime: '2026-02-01T00:00:00Z',
        EndTime: '2026-08-15T00:00:00Z',
        ProjectStatus: 'COMPLETED',
    },
    {
        ProjectID: 'PRJ-003',
        CatID: null,
        Title: '校园猫咪冬季御寒猫窝计划',
        TargetAmount: 8000,
        RaisedAmount: 6120,
        StartTime: '2026-03-01T00:00:00Z',
        EndTime: '2026-11-30T00:00:00Z',
        ProjectStatus: 'ACTIVE',
    },
    {
        ProjectID: 'PRJ-004',
        CatID: 'CAT-004',
        Title: '三花娘绝育手术',
        TargetAmount: 1200,
        RaisedAmount: 840,
        StartTime: '2026-07-01T00:00:00Z',
        EndTime: '2026-09-01T00:00:00Z',
        ProjectStatus: 'CANCELLED',
    },
    {
        ProjectID: 'PRJ-005',
        CatID: 'CAT-005',
        Title: '图书馆猫粮补给计划',
        TargetAmount: 3000,
        RaisedAmount: 150,
        StartTime: '2026-08-01T00:00:00Z',
        EndTime: '2026-12-01T00:00:00Z',
        ProjectStatus: 'ACTIVE',
    },
]

// ═══════════════════════════════════════════
// 2. 支出记录
// ═══════════════════════════════════════════
export const mockExpenses: ExpenseRecord[] = [
    {
        FinanceID: 'FIN-001',
        ProjectID: 'PRJ-001',
        RecordType: 'MEDICAL',
        Amount: 1200,
        InvoiceURL: 'https://example.com/invoices/fin-001.pdf',
        AuditUserID: 'USR-ADMIN',
        AuditStatus: 'APPROVED',
        PublicTime: '2026-03-10T08:00:00Z',
    },
    {
        FinanceID: 'FIN-002',
        ProjectID: 'PRJ-001',
        RecordType: 'FOOD',
        Amount: 350,
        InvoiceURL: 'https://example.com/invoices/fin-002.pdf',
        AuditUserID: 'USR-ADMIN',
        AuditStatus: 'APPROVED',
        PublicTime: '2026-04-22T08:00:00Z',
    },
    {
        FinanceID: 'FIN-003',
        ProjectID: 'PRJ-002',
        RecordType: 'MEDICAL',
        Amount: 800,
        InvoiceURL: 'https://example.com/invoices/fin-003.pdf',
        AuditUserID: 'USR-ADMIN',
        AuditStatus: 'APPROVED',
        PublicTime: '2026-05-01T08:00:00Z',
    },
    {
        FinanceID: 'FIN-004',
        ProjectID: 'PRJ-003',
        RecordType: 'SUPPLIES',
        Amount: 2500,
        InvoiceURL: 'https://example.com/invoices/fin-004.pdf',
        AuditUserID: 'USR-ADMIN',
        AuditStatus: 'APPROVED',
        PublicTime: '2026-06-15T08:00:00Z',
    },
    {
        FinanceID: 'FIN-005',
        ProjectID: 'PRJ-003',
        RecordType: 'SUPPLIES',
        Amount: 880,
        InvoiceURL: '',
        AuditUserID: null,
        AuditStatus: 'PENDING',
        PublicTime: null,
    },
    {
        FinanceID: 'FIN-006',
        ProjectID: 'PRJ-001',
        RecordType: 'OTHER',
        Amount: 200,
        InvoiceURL: 'https://example.com/invoices/fin-006.pdf',
        AuditUserID: null,
        AuditStatus: 'PENDING',
        PublicTime: null,
    },
    {
        FinanceID: 'FIN-007',
        ProjectID: 'PRJ-005',
        RecordType: 'FOOD',
        Amount: 450,
        InvoiceURL: 'https://example.com/invoices/fin-007.pdf',
        AuditUserID: 'USR-ADMIN',
        AuditStatus: 'REJECTED',
        PublicTime: null,
    },
]

// ═══════════════════════════════════════════
// 3. 捐赠记录
// ═══════════════════════════════════════════
export const mockDonations: DonationRecord[] = [
    {
        DonationID: 'DON-001',
        ProjectID: 'PRJ-001',
        DonorUserID: 'USR-1001',
        Amount: 500,
        PayMethod: 'WECHAT',
        PayTime: '2026-02-20T14:30:00Z',
        PublicFlag: 1,
    },
    {
        DonationID: 'DON-002',
        ProjectID: 'PRJ-001',
        DonorUserID: 'USR-1002',
        Amount: 200,
        PayMethod: 'ALIPAY',
        PayTime: '2026-03-01T10:15:00Z',
        PublicFlag: 1,
    },
    {
        DonationID: 'DON-003',
        ProjectID: 'PRJ-001',
        DonorUserID: null,
        Amount: 100,
        PayMethod: 'WECHAT',
        PayTime: '2026-03-15T09:00:00Z',
        PublicFlag: 0, // 匿名
    },
    {
        DonationID: 'DON-004',
        ProjectID: 'PRJ-002',
        DonorUserID: 'USR-1003',
        Amount: 800,
        PayMethod: 'BANK_TRANSFER',
        PayTime: '2026-03-10T16:00:00Z',
        PublicFlag: 1,
    },
    {
        DonationID: 'DON-005',
        ProjectID: 'PRJ-002',
        DonorUserID: 'USR-1001',
        Amount: 300,
        PayMethod: 'ALIPAY',
        PayTime: '2026-04-05T12:00:00Z',
        PublicFlag: 1,
    },
    {
        DonationID: 'DON-006',
        ProjectID: 'PRJ-003',
        DonorUserID: 'USR-1004',
        Amount: 1500,
        PayMethod: 'CASH',
        PayTime: '2026-04-18T11:30:00Z',
        PublicFlag: 1,
    },
    {
        DonationID: 'DON-007',
        ProjectID: 'PRJ-003',
        DonorUserID: 'USR-1005',
        Amount: 600,
        PayMethod: 'WECHAT',
        PayTime: '2026-05-22T19:45:00Z',
        PublicFlag: 0,
    },
    {
        DonationID: 'DON-008',
        ProjectID: 'PRJ-005',
        DonorUserID: 'USR-1002',
        Amount: 150,
        PayMethod: 'ALIPAY',
        PayTime: '2026-08-03T08:20:00Z',
        PublicFlag: 1,
    },
]

// ═══════════════════════════════════════════
// 4. 财务公示摘要（仅 ACTIVE 项目）
// ═══════════════════════════════════════════
export const mockDisclosureSummary: FinancialDisclosureSummary[] = (() => {
    // 按 project 聚合 expenses / donations 计算出 summary
    function sumExpenseApproved(pid: string): number {
        return mockExpenses
            .filter((e) => e.ProjectID === pid && e.AuditStatus === 'APPROVED')
            .reduce((s, e) => s + e.Amount, 0)
    }
    function countDonations(pid: string): number {
        return mockDonations.filter((d) => d.ProjectID === pid).length
    }

    return mockProjects
        .filter((p) => p.ProjectStatus === 'ACTIVE')
        .map((p) => {
            const raised = p.RaisedAmount ?? 0
            const expense = sumExpenseApproved(p.ProjectID)
            return {
                Project: p,
                TargetAmount: p.TargetAmount,
                RaisedAmount: raised,
                TotalExpense: expense,
                NetBalance: raised - expense,
                DonationCount: countDonations(p.ProjectID),
            } satisfies FinancialDisclosureSummary
        })
})()

// ═══════════════════════════════════════════
// 5. 统计快照
// ═══════════════════════════════════════════
export const mockSnapshots: SnapshotRecord[] = [
    // PRJ-001 橘座口炎治疗
    { SnapshotID: 'SNAP-001', SnapshotDate: '2026-03-01T00:00:00Z', MetricCode: 'TOTAL_DONATION', MetricValue: 800, DimensionType: 'PROJECT', DimensionValue: 'PRJ-001', Unit: 'CNY', GenerateTime: '2026-03-01T12:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-002', SnapshotDate: '2026-03-01T00:00:00Z', MetricCode: 'TOTAL_EXPENSE', MetricValue: 1200, DimensionType: 'PROJECT', DimensionValue: 'PRJ-001', Unit: 'CNY', GenerateTime: '2026-03-01T12:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-003', SnapshotDate: '2026-03-01T00:00:00Z', MetricCode: 'NET_BALANCE', MetricValue: 3850 - 1550, DimensionType: 'PROJECT', DimensionValue: 'PRJ-001', Unit: 'CNY', GenerateTime: '2026-03-01T12:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-004', SnapshotDate: '2026-03-01T00:00:00Z', MetricCode: 'DONATION_COUNT', MetricValue: 3, DimensionType: 'PROJECT', DimensionValue: 'PRJ-001', Unit: 'COUNT', GenerateTime: '2026-03-01T12:00:00Z', Remark: null },
    // PRJ-003 猫窝计划
    { SnapshotID: 'SNAP-005', SnapshotDate: '2026-06-01T00:00:00Z', MetricCode: 'TOTAL_DONATION', MetricValue: 2100, DimensionType: 'PROJECT', DimensionValue: 'PRJ-003', Unit: 'CNY', GenerateTime: '2026-06-01T12:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-006', SnapshotDate: '2026-06-01T00:00:00Z', MetricCode: 'TOTAL_EXPENSE', MetricValue: 2500, DimensionType: 'PROJECT', DimensionValue: 'PRJ-003', Unit: 'CNY', GenerateTime: '2026-06-01T12:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-007', SnapshotDate: '2026-06-01T00:00:00Z', MetricCode: 'NET_BALANCE', MetricValue: -400, DimensionType: 'PROJECT', DimensionValue: 'PRJ-003', Unit: 'CNY', GenerateTime: '2026-06-01T12:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-008', SnapshotDate: '2026-06-01T00:00:00Z', MetricCode: 'DONATION_COUNT', MetricValue: 2, DimensionType: 'PROJECT', DimensionValue: 'PRJ-003', Unit: 'COUNT', GenerateTime: '2026-06-01T12:00:00Z', Remark: null },
    // 按月维度
    { SnapshotID: 'SNAP-009', SnapshotDate: '2026-03-01T00:00:00Z', MetricCode: 'TOTAL_DONATION', MetricValue: 1800, DimensionType: 'MONTH', DimensionValue: '2026-03', Unit: 'CNY', GenerateTime: '2026-04-01T00:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-010', SnapshotDate: '2026-04-01T00:00:00Z', MetricCode: 'TOTAL_DONATION', MetricValue: 2100, DimensionType: 'MONTH', DimensionValue: '2026-04', Unit: 'CNY', GenerateTime: '2026-05-01T00:00:00Z', Remark: null },
    { SnapshotID: 'SNAP-011', SnapshotDate: '2026-05-01T00:00:00Z', MetricCode: 'TOTAL_DONATION', MetricValue: 600, DimensionType: 'MONTH', DimensionValue: '2026-05', Unit: 'CNY', GenerateTime: '2026-06-01T00:00:00Z', Remark: null },
]

// ═══════════════════════════════════════════
// 6. 项目明细（Mock）
// ═══════════════════════════════════════════
export function buildMockDisclosureDetail(projectId: string): FinancialDisclosureDetail {
    const project = mockProjects.find((p) => p.ProjectID === projectId)
    if (!project) {
        throw new Error(`未找到项目 ${projectId}`)
    }
    const donations = mockDonations.filter((d) => d.ProjectID === projectId)
    const expenses = mockExpenses.filter((e) => e.ProjectID === projectId)
    const totalExpense = expenses
        .filter((e) => e.AuditStatus === 'APPROVED')
        .reduce((s, e) => s + e.Amount, 0)
    const raised = project.RaisedAmount ?? 0

    return {
        Project: project,
        TargetAmount: project.TargetAmount,
        RaisedAmount: raised,
        TotalExpense: totalExpense,
        NetBalance: raised - totalExpense,
        DonationCount: donations.length,
        Donations: donations,
        Expenses: expenses,
    }
}
