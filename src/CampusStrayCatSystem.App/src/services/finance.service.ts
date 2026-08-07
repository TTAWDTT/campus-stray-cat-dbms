import { http } from './http'

type ApiRecord = Record<string, unknown>

const value = <T>(data: ApiRecord, camel: string, pascal: string): T | undefined =>
    (data[camel] ?? data[pascal]) as T | undefined

//支出记录
export interface ExpenseRecord {
    FinanceID: string
    ProjectID: string
    RecordType: string
    Amount: number
    InvoiceURL: string
    AuditUserID: string | null
    AuditStatus: 'PENDING' | 'APPROVED' | 'REJECTED'
    PublicTime: string | null
}

const toExpense = (data: ApiRecord): ExpenseRecord => ({
    FinanceID: value<string>(data, 'financeID', 'FinanceID') || '',
    ProjectID: value<string>(data, 'projectID', 'ProjectID') || '',
    RecordType: value<string>(data, 'recordType', 'RecordType') || 'OTHER',
    Amount: value<number>(data, 'amount', 'Amount') || 0,
    InvoiceURL: value<string>(data, 'invoiceURL', 'InvoiceURL') ?? value<string>(data, 'invoiceUrl', 'InvoiceUrl') ?? '',
    AuditUserID: value<string | null>(data, 'auditUserID', 'AuditUserID') ?? null,
    AuditStatus: (value<string>(data, 'auditStatus', 'AuditStatus') || 'PENDING') as ExpenseRecord['AuditStatus'],
    PublicTime: value<string | null>(data, 'publicTime', 'PublicTime') ?? null,
})

export interface CreateExpensePayload {
    ProjectID: string
    RecordType: string
    Amount: number
    InvoiceUrl: string
}

//捐赠记录
export interface DonationRecord {
    DonationID: string
    ProjectID: string
    DonorUserID: string | null
    Amount: number
    PayMethod: string
    PayTime: string | null
    PublicFlag: number
}

const toDonation = (data: ApiRecord): DonationRecord => ({
    DonationID: value<string>(data, 'donationID', 'DonationID') || '',
    ProjectID: value<string>(data, 'projectID', 'ProjectID') || '',
    DonorUserID: value<string | null>(data, 'donorUserID', 'DonorUserID') ?? null,
    Amount: value<number>(data, 'amount', 'Amount') || 0,
    PayMethod: value<string>(data, 'payMethod', 'PayMethod') || 'OTHER',
    PayTime: value<string | null>(data, 'payTime', 'PayTime') ?? null,
    PublicFlag: value<number>(data, 'publicFlag', 'PublicFlag') ?? 0,
})

export interface CreateDonationPayload {
    ProjectID: string
    DonorUserID?: string
    Amount: number
    PayMethod?: string
    PayTime?: string
    PublicFlag?: number
}

//众筹项目
export interface CrowdfundingProject {
    ProjectID: string
    CatID: string | null
    Title: string
    TargetAmount: number | null
    RaisedAmount: number | null
    StartTime: string | null
    EndTime: string | null
    ProjectStatus: 'ACTIVE' | 'COMPLETED' | 'CANCELLED'
}

const toProject = (data: ApiRecord): CrowdfundingProject => ({
    ProjectID: value<string>(data, 'projectID', 'ProjectID') || '',
    CatID: value<string | null>(data, 'catID', 'CatID') ?? null,
    Title: value<string>(data, 'title', 'Title') || '',
    TargetAmount: value<number | null>(data, 'targetAmount', 'TargetAmount') ?? null,
    RaisedAmount: value<number | null>(data, 'raisedAmount', 'RaisedAmount') ?? null,
    StartTime: value<string | null>(data, 'startTime', 'StartTime') ?? null,
    EndTime: value<string | null>(data, 'endTime', 'EndTime') ?? null,
    ProjectStatus: (value<string>(data, 'projectStatus', 'ProjectStatus') || 'ACTIVE') as CrowdfundingProject['ProjectStatus'],
})

export interface CreateProjectPayload {
    Title: string
    CatID?: string | null
    TargetAmount?: number | null
    StartTime?: string | null
    EndTime?: string | null
    ProjectStatus?: 'ACTIVE' | 'COMPLETED' | 'CANCELLED'
}

export interface UpdateProjectPayload {
    ProjectID?: string
    Title?: string
    CatID?: string | null
    TargetAmount?: number | null
    StartTime?: string | null
    EndTime?: string | null
    ProjectStatus?: 'ACTIVE' | 'COMPLETED' | 'CANCELLED'
}

//财务公示摘要
export interface FinancialDisclosureSummary {
    Project: CrowdfundingProject
    TargetAmount: number | null
    RaisedAmount: number | null
    TotalExpense: number | null
    NetBalance: number | null
    DonationCount: number
}

const toDisclosureSummary = (data: ApiRecord): FinancialDisclosureSummary => {
    const projectData = (data.Project ?? data.project) as ApiRecord | undefined
    return {
        Project: projectData ? toProject(projectData) : { ProjectID: '', CatID: null, Title: '', TargetAmount: null, RaisedAmount: null, StartTime: null, EndTime: null, ProjectStatus: 'ACTIVE' },
        TargetAmount: value<number | null>(data, 'targetAmount', 'TargetAmount') ?? null,
        RaisedAmount: value<number | null>(data, 'raisedAmount', 'RaisedAmount') ?? null,
        TotalExpense: value<number | null>(data, 'totalExpense', 'TotalExpense') ?? null,
        NetBalance: value<number | null>(data, 'netBalance', 'NetBalance') ?? null,
        DonationCount: value<number>(data, 'donationCount', 'DonationCount') ?? 0,
    }
}

// 财务公示详情
export interface FinancialDisclosureDetail {
    Project: CrowdfundingProject
    TargetAmount: number | null
    RaisedAmount: number | null
    TotalExpense: number | null
    NetBalance: number | null
    DonationCount: number
    Donations: DonationRecord[]
    Expenses: ExpenseRecord[]
}

const toDisclosureDetail = (data: ApiRecord): FinancialDisclosureDetail => {
    const projectData = (data.Project ?? data.project) as ApiRecord | undefined
    const donations = (data.Donations ?? data.donations) as ApiRecord[] | undefined
    const expenses = (data.Expenses ?? data.expenses) as ApiRecord[] | undefined
    return {
        Project: projectData ? toProject(projectData) : { ProjectID: '', CatID: null, Title: '', TargetAmount: null, RaisedAmount: null, StartTime: null, EndTime: null, ProjectStatus: 'ACTIVE' },
        TargetAmount: value<number | null>(data, 'targetAmount', 'TargetAmount') ?? null,
        RaisedAmount: value<number | null>(data, 'raisedAmount', 'RaisedAmount') ?? null,
        TotalExpense: value<number | null>(data, 'totalExpense', 'TotalExpense') ?? null,
        NetBalance: value<number | null>(data, 'netBalance', 'NetBalance') ?? null,
        DonationCount: value<number>(data, 'donationCount', 'DonationCount') ?? 0,
        Donations: donations ? donations.map(toDonation) : [],
        Expenses: expenses ? expenses.map(toExpense) : [],
    }
}

export const financeService = {
    //支出记录
    async listExpenses() {
        const { data } = await http.get<ApiRecord[]>('/expense-records')
        return data.map(toExpense)
    },

    async getExpense(id: string) {
        const { data } = await http.get<ApiRecord>(`/expense-records/${encodeURIComponent(id)}`)
        return toExpense(data)
    },

    async getExpensesByProject(projectId: string) {
        const { data } = await http.get<ApiRecord[]>(`/expense-records/by-project/${encodeURIComponent(projectId)}`)
        return data.map(toExpense)
    },

    async createExpense(payload: CreateExpensePayload) {
        const { data } = await http.post<ApiRecord>('/expense-records', payload)
        return toExpense(data)
    },

    async auditExpense(id: string, auditStatus: 'APPROVED' | 'REJECTED') {
        await http.put(`/expense-records/${encodeURIComponent(id)}/audit`, { auditStatus })
    },

    //捐赠记录
    async listDonations() {
        const { data } = await http.get<ApiRecord[]>('/donations')
        return data.map(toDonation)
    },

    async getDonation(id: string) {
        const { data } = await http.get<ApiRecord>(`/donations/${encodeURIComponent(id)}`)
        return toDonation(data)
    },

    async getDonationsByProject(projectId: string) {
        const { data } = await http.get<ApiRecord[]>(`/donations/by-project/${encodeURIComponent(projectId)}`)
        return data.map(toDonation)
    },

    async getDonationsByDonor(donorUserId: string) {
        const { data } = await http.get<ApiRecord[]>(`/donations/by-donor/${encodeURIComponent(donorUserId)}`)
        return data.map(toDonation)
    },

    async createDonation(payload: CreateDonationPayload) {
        const { data } = await http.post<ApiRecord>('/donations', payload)
        return toDonation(data)
    },

    // 众筹项目
    async listProjects() {
        const { data } = await http.get<ApiRecord[]>('/crowdfunding-projects')
        return data.map(toProject)
    },

    async getProject(id: string) {
        const { data } = await http.get<ApiRecord>(`/crowdfunding-projects/${encodeURIComponent(id)}`)
        return toProject(data)
    },

    async getProjectsByStatus(status: string) {
        const { data } = await http.get<ApiRecord[]>(`/crowdfunding-projects/by-status/${encodeURIComponent(status)}`)
        return data.map(toProject)
    },

    async createProject(payload: CreateProjectPayload) {
        const { data } = await http.post<ApiRecord>('/crowdfunding-projects', payload)
        return toProject(data)
    },

    async updateProject(id: string, payload: UpdateProjectPayload) {
        await http.put(`/crowdfunding-projects/${encodeURIComponent(id)}`, payload)
    },

    async catExists(catId: string) {
        try {
            await http.get(`/cats/${encodeURIComponent(catId)}`)
            return true
        } catch (error: any) {
            if (error.response && error.response.status === 404) {
                return false
            }
            throw error
        }
    },

    //财务公示
    async getDisclosureSummary() {
        const { data } = await http.get<ApiRecord[]>('/financial-disclosure/summary')
        return data.map(toDisclosureSummary)
    },

    async getDisclosureDetail(projectId: string) {
        const { data } = await http.get<ApiRecord>(`/financial-disclosure/${encodeURIComponent(projectId)}`)
        return toDisclosureDetail(data)
    },

    // 统计快照
    async listSnapshots() {
        const { data } = await http.get<ApiRecord[]>('/statistics-reports')
        return data.map(toSnapshot)
    },

    async getSnapshot(id: string) {
        const { data } = await http.get<ApiRecord>(`/statistics-reports/snapshot/${encodeURIComponent(id)}`)
        return toSnapshot(data)
    },

    async getSnapshotsByMetric(metricCode: string) {
        const { data } = await http.get<ApiRecord[]>(`/statistics-reports/by-metric/${encodeURIComponent(metricCode)}`)
        return data.map(toSnapshot)
    },

    async generateProjectReport(projectId: string) {
        const { data } = await http.post<ApiRecord>(`/statistics-reports/generate/${encodeURIComponent(projectId)}`)
        return data as unknown as { message: string; projectId: string; projectTitle: string; metrics: { totalDonation: number; totalExpense: number; netBalance: number; donationCount: number } }
    },
}

//统计快照
export interface SnapshotRecord {
    SnapshotID: string
    SnapshotDate: string | null
    MetricCode: string
    MetricValue: number | null
    DimensionType: string | null
    DimensionValue: string | null
    Unit: string | null
    GenerateTime: string | null
    Remark: string | null
}

const metricLabels: Record<string, string> = {
    TOTAL_DONATION: '总捐赠',
    TOTAL_EXPENSE: '总支出',
    NET_BALANCE: '净余额',
    DONATION_COUNT: '捐赠笔数',
}

const dimensionLabels: Record<string, string> = {
    PROJECT: '项目',
    MONTH: '月份',
    CAT: '猫咪',
}

const toSnapshot = (data: ApiRecord): SnapshotRecord => ({
    SnapshotID: value<string>(data, 'snapshotID', 'SnapshotID') || '',
    SnapshotDate: value<string | null>(data, 'snapshotDate', 'SnapshotDate') ?? null,
    MetricCode: value<string>(data, 'metricCode', 'MetricCode') || '',
    MetricValue: value<number | null>(data, 'metricValue', 'MetricValue') ?? null,
    DimensionType: value<string | null>(data, 'dimensionType', 'DimensionType') ?? null,
    DimensionValue: value<string | null>(data, 'dimensionValue', 'DimensionValue') ?? null,
    Unit: value<string | null>(data, 'unit', 'Unit') ?? null,
    GenerateTime: value<string | null>(data, 'generateTime', 'GenerateTime') ?? null,
    Remark: value<string | null>(data, 'remark', 'Remark') ?? null,
})

export { metricLabels, dimensionLabels }
