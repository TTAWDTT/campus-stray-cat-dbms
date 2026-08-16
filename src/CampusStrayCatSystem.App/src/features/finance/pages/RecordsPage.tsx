import { useNavigate } from 'react-router-dom'
import {
    Button, Card, Icon, Table, type TableColumn,
    Tabs, Collapse, Tag,
} from 'animal-island-ui'
import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import { financeService } from '../../../services/finance.service'
import type { ExpenseRecord, DonationRecord, CreateExpensePayload, CreateDonationPayload, FinancialDisclosureSummary } from '../../../services/finance.service'
import { FinanceSummaryCard } from '../components/FinanceSummaryCard'
import { CreateRecordDrawer } from '../components/CreateRecordDrawer'
import { mockExpenses, mockDonations, mockDisclosureSummary, USE_MOCK } from '../test/mockData'
import { useAuthStore } from '../../../stores/auth.store'
import { PageHeader } from '../../../shared/components/PageHeader'
//本页面用于展示所有的捐款与支出记录，管理员可以审核支出记录，普通用户可以查看自己的捐款记录。页面提供了一个财务公示卡片，显示总收入、总支出和当前余额，并使用表格展示详细的记录信息。
const readError = (error: unknown): string => {
    if (axios.isAxiosError(error)) {
        const msg = error.response?.data
        if (typeof msg?.message === 'string') return msg.message
        if (typeof msg === 'string') return msg
        if (error.response?.status === 403) return '没有操作权限。'
        if (error.response?.status === 401) return '登录已过期，请重新登录。'
    }
    return '网络异常，请稍后重试。'
}

const castExpense = (row: Record<string, unknown>) => row as unknown as ExpenseRecord
const castDonation = (row: Record<string, unknown>) => row as unknown as DonationRecord

const statusLabel: Record<string, string> = {
    PENDING: '待审核',
    APPROVED: '已通过',
    REJECTED: '已驳回',
}

const recordTypeLabel: Record<string, string> = {
    FOOD: '食物',
    MEDICAL: '医疗',
    SUPPLIES: '物资',
    OTHER: '其他',
}

const payMethodLabel: Record<string, string> = {
    ALIPAY: '支付宝',
    WECHAT: '微信',
    BANK_TRANSFER: '银行转账',
    CASH: '现金',
    OTHER: '其他',
}

const downloadExcel = (filename: string, headers: string[], rows: Array<Array<string | number>>) => {
    const cell = (value: string | number) => `<td>${String(value).replace(/[&<>]/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;' })[char] || char)}</td>`
    const html = `<table><thead><tr>${headers.map((header) => `<th>${header}</th>`).join('')}</tr></thead><tbody>${rows.map((row) => `<tr>${row.map(cell).join('')}</tr>`).join('')}</tbody></table>`
    const blob = new Blob([`<html><meta charset="utf-8">${html}</html>`], { type: 'application/vnd.ms-excel' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `${filename}.xls`
    anchor.click()
    URL.revokeObjectURL(url)
}

const exportRecordsToExcel = (activeKey: string, expenses: ExpenseRecord[], donations: DonationRecord[], myDonations: DonationRecord[], checkRecords: ExpenseRecord[]) => {
    if (activeKey === 'payment' || activeKey === 'check') {
        const source = activeKey === 'check' ? checkRecords : expenses
        downloadExcel('财务支出记录', ['记录 ID', '项目 ID', '类型', '金额', '审核状态', '公开时间'], source.map((item) => [item.FinanceID, item.ProjectID, recordTypeLabel[item.RecordType] || item.RecordType, item.Amount, statusLabel[item.AuditStatus] || item.AuditStatus, item.PublicTime || '—']))
        return
    }
    const source = activeKey === 'myDonations' ? myDonations : donations
    downloadExcel('财务捐款记录', ['捐款 ID', '项目 ID', '金额', '支付方式', '支付时间', '公开状态'], source.map((item) => [item.DonationID, item.ProjectID, item.Amount, payMethodLabel[item.PayMethod] || item.PayMethod, item.PayTime || '—', item.PublicFlag === 1 ? '公开' : '匿名']))
}

// 捐款列
const donationColumns: TableColumn[] = [
    { title: '捐款ID', dataIndex: 'DonationID' },
    {
        title: '捐款金额',
        dataIndex: 'Amount',
        render: (_v, row) => <span>¥{castDonation(row).Amount?.toLocaleString() ?? '0'}</span>,
    },
    { title: '捐款时间', dataIndex: 'PayTime' },
    {
        title: '支付方式',
        dataIndex: 'PayMethod',
        render: (_v, row) => {
            const m = castDonation(row).PayMethod
            return <span>{payMethodLabel[m] || m || '-'}</span>
        },
    },
    {
        title: '详细信息',
        width: 130,
        render: (_v, row) => {
            const r = castDonation(row)
            return (
                <Collapse className="table-collapse" question="详情" answer={
                    <div className="collapse-detail">
                        <div>捐款人ID: {r.PublicFlag === 1 ? r.DonorUserID : '匿名'}</div>
                        <div>项目ID: {r.ProjectID}</div>
                        <div>支付方式: {payMethodLabel[r.PayMethod] || r.PayMethod}</div>
                    </div>
                } />
            )
        },
    },
]

// 支出列
const paymentColumns: TableColumn[] = [
    { title: '支出ID', dataIndex: 'FinanceID' },
    {
        title: '记录类型',
        dataIndex: 'RecordType',
        render: (_v, row) => {
            const t = castExpense(row).RecordType
            return <span>{recordTypeLabel[t] || t || '-'}</span>
        },
    },
    {
        title: '金额',
        dataIndex: 'Amount',
        render: (_v, row) => <span>¥{castExpense(row).Amount?.toLocaleString() ?? '0'}</span>,
    },
    {
        title: '审核状态',
        dataIndex: 'AuditStatus',
        render: (_v, row) => {
            const r = castExpense(row)
            const s = r.AuditStatus || 'PENDING'
            const color = s === 'APPROVED' ? 'app-green' : s === 'REJECTED' ? 'app-red' : 'app-yellow'
            return <Tag color={color} variant="soft">{statusLabel[s] || s}</Tag>
        },
    },
    { title: '公开时间', dataIndex: 'PublicTime' },
    {
        title: '详细信息',
        width: 130,
        render: (_v, row) => {
            const r = castExpense(row)
            return (
                <Collapse className="table-collapse" question="详情" answer={
                    <div className="collapse-detail">
                        <div>支出ID: {r.FinanceID}</div>
                        <div>类型: {recordTypeLabel[r.RecordType] || r.RecordType}</div>
                        <div>金额: ¥{r.Amount?.toLocaleString() ?? '0'}</div>
                        <div>状态: {statusLabel[r.AuditStatus] || r.AuditStatus}</div>
                        <div>发票: {r.InvoiceURL || '-'}</div>
                    </div>
                } />
            )
        },
    },
]

// 组件
export function RecordsPage({ embedded = false }: { embedded?: boolean }) {
    const navigate = useNavigate()
    const [activeKey, setActiveKey] = useState('myDonations')
    const [drawerOpen, setDrawerOpen] = useState(false)

    // 数据
    const [expenses, setExpenses] = useState<ExpenseRecord[]>([])
    const [donations, setDonations] = useState<DonationRecord[]>([])
    const [summary, setSummary] = useState<FinancialDisclosureSummary[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')

    // 当前用户
    const user = useAuthStore((s) => s.user)
    const isAdmin = user?.roleName?.toUpperCase() === 'ADMIN'
    const canManageExpenses = ['ADMIN', 'VOLUNTEER'].includes((user?.roleName || '').toUpperCase())

    // 我的捐赠 = 当前用户的捐款记录
    const myDonations = useMemo(
        () => user ? donations.filter((d) => d.DonorUserID === user.userId) : [],
        [donations, user],
    )

    // 待审核 = 支出中 AuditStatus === PENDING
    const checkRecords = useMemo(
        () => expenses.filter((r) => r.AuditStatus === 'PENDING'),
        [expenses],
    )

    // ── 数据加载 ──
    const loadData = async () => {
        setLoading(true)
        setError('')
        try {
            if (USE_MOCK) {
                // 模拟 500ms 网络延迟
                await new Promise((r) => setTimeout(r, 500))
                setSummary(mockDisclosureSummary)
                if (canManageExpenses) {
                    setExpenses(mockExpenses)
                } else {
                    setExpenses([])
                }
                if (isAdmin) {
                    setDonations(mockDonations)
                } else {
                    const donorId = user?.userId
                    setDonations(donorId ? mockDonations.filter((d) => d.DonorUserID === donorId) : [])
                }
            } else {
                if (canManageExpenses) {
                    const exp = await financeService.listExpenses()
                    setExpenses(exp)
                } else {
                    setExpenses([])
                }
                if (isAdmin) {
                    const [don, sum] = await Promise.all([
                        financeService.listDonations(),
                        financeService.getDisclosureSummary(),
                    ])
                    setDonations(don)
                    setSummary(sum)
                } else {
                    const donorId = user?.userId
                    const [don, sum] = await Promise.all([
                        donorId ? financeService.getDonationsByDonor(donorId) : Promise.resolve([]),
                        financeService.getDisclosureSummary(),
                    ])
                    setDonations(don)
                    setSummary(sum)
                }
            }
        } catch (e) {
            setError(readError(e))
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => { loadData() }, [])

    // 审核操作
    const handleAudit = async (financeId: string, status: 'APPROVED' | 'REJECTED') => {
        setError('')
        try {
            if (USE_MOCK) {
                // 模拟审核：直接更新本地状态
                setExpenses((prev) =>
                    prev.map((r) =>
                        r.FinanceID === financeId
                            ? { ...r, AuditStatus: status, PublicTime: status === 'APPROVED' ? new Date().toISOString() : r.PublicTime }
                            : r,
                    ),
                )
            } else {
                await financeService.auditExpense(financeId, status)
                await loadData()
            }
        } catch (e) {
            setError(readError(e))
        }
    }

    // 待审核列
    const checkColumns: TableColumn[] = useMemo(() => [
        { title: '支出ID', dataIndex: 'FinanceID' },
        { title: '记录类型', dataIndex: 'RecordType', render: (_v, row) => <span>{recordTypeLabel[castExpense(row).RecordType] || '-'}</span> },
        { title: '金额', dataIndex: 'Amount', render: (_v, row) => <span>¥{castExpense(row).Amount?.toLocaleString() ?? '0'}</span> },
        { title: '审核状态', dataIndex: 'AuditStatus', render: (_v, row) => <Tag color="app-yellow" variant="soft">{statusLabel[castExpense(row).AuditStatus] || '待审核'}</Tag> },
        { title: '发票链接', dataIndex: 'InvoiceURL' },
        {
            title: '详细信息',
            width: 130,
            render: (_v, row) => {
                const r = castExpense(row)
                return <Collapse className="table-collapse" question="详情" answer={<div className="collapse-detail"><div>支出ID: {r.FinanceID}</div><div>类型: {recordTypeLabel[r.RecordType] || r.RecordType}</div><div>金额: ¥{r.Amount?.toLocaleString() ?? '0'}</div><div>发票: {r.InvoiceURL || '-'}</div></div>} />
            },
        },
        {
            title: '操作',
            width: 200,
            render: (_v, row) => {
                const r = castExpense(row)
                return (
                    <div style={{ display: 'flex', gap: 6 }}>
                        <Button type="default" size="small" onClick={() => handleAudit(r.FinanceID, 'APPROVED')}>通过</Button>
                        <Button type="default" size="small" onClick={() => handleAudit(r.FinanceID, 'REJECTED')}>拒绝</Button>
                    </div>
                )
            },
        },
    ], [])

    // 创建支出回调（由 CreateRecordDrawer 调用）
    const handleCreateExpense = async (payload: CreateExpensePayload) => {
        setError('')
        if (USE_MOCK) {
            const newExpense: ExpenseRecord = {
                FinanceID: `FIN-MOCK-${Date.now()}`,
                ProjectID: payload.ProjectID,
                RecordType: payload.RecordType,
                Amount: payload.Amount,
                InvoiceURL: payload.InvoiceUrl,
                AuditUserID: null,
                AuditStatus: 'PENDING',
                PublicTime: null,
            }
            setExpenses((prev) => [newExpense, ...prev])
        } else {
            await financeService.createExpense(payload)
            await loadData()
        }
    }

    // 创建捐款回调（由 CreateRecordDrawer 调用）
    const handleCreateDonation = async (payload: CreateDonationPayload) => {
        setError('')
        if (USE_MOCK) {
            const newDonation: DonationRecord = {
                DonationID: `DON-MOCK-${Date.now()}`,
                ProjectID: payload.ProjectID,
                DonorUserID: payload.DonorUserID ?? null,
                Amount: payload.Amount,
                PayMethod: payload.PayMethod ?? 'OTHER',
                PayTime: payload.PayTime ?? null,
                PublicFlag: payload.PublicFlag ?? 0,
            }
            setDonations((prev) => [newDonation, ...prev])
        } else {
            await financeService.createDonation(payload)
            await loadData()
        }
    }

    return (
        <section className={`feature-page finance-records-page${embedded ? ' finance-embedded-page' : ''}`}>
            {embedded ? <div className="finance-embedded-toolbar"><div><strong>财务记录</strong><small>捐款、支出和审核状态</small></div><div><Button type="default" size="small" onClick={() => window.print()}>导出 PDF</Button><Button type="default" size="small" onClick={() => exportRecordsToExcel(activeKey, expenses, donations, myDonations, checkRecords)}>导出 Excel</Button></div></div> : <PageHeader kicker="FINANCE · RECORDS" title="财务记录" icon="icon-camera" actions={<Button type="text" size="small" onClick={() => navigate('/finance')}><Icon name="icon-miles" size={15} />返回</Button>} />}

            {error && (
                <div className="cats-alert" role="alert">
                    <Icon name="icon-camera" size={17} />
                    <span>{error}</span>
                    <Button type="text" size="small" onClick={() => setError('')}>知道了</Button>
                </div>
            )}

            <FinanceSummaryCard data={summary} loading={loading} />

            <Card className="cats-table-card finance-records-card">
                <div className="finance-records-toolbar">
                    <span className="finance-records-toolbar-title">资金流水</span>
                    {((canManageExpenses && activeKey === 'payment') || (isAdmin && activeKey === 'donation')) && (
                        <Button type="default" size="small" onClick={() => setDrawerOpen(true)}>
                            <span>新建{activeKey === 'payment' ? '支出' : '捐款'}</span>
                        </Button>
                    )}
                </div>
                <Tabs
                    activeKey={activeKey}
                    onChange={setActiveKey}
                    items={[
                        // 管理员/志愿者：支出记录
                        ...(canManageExpenses
                            ? [
                                {
                                    key: 'payment',
                                    label: '支出记录',
                                    children: (
                                        <Table
                                            columns={paymentColumns}
                                            dataSource={expenses as unknown as Record<string, unknown>[]}
                                            rowKey="FinanceID"
                                            loading={loading}
                                            emptyText="暂无支出记录"
                                        />
                                    ),
                                } as const,
                            ]
                            : []),
                        // 管理员：捐款记录、待审核
                        ...(isAdmin
                            ? [
                                {
                                    key: 'donation',
                                    label: '捐款记录',
                                    children: (
                                        <Table
                                            columns={donationColumns}
                                            dataSource={donations as unknown as Record<string, unknown>[]}
                                            rowKey="DonationID"
                                            loading={loading}
                                            emptyText="暂无捐款记录"
                                        />
                                    ),
                                } as const,
                                {
                                    key: 'check',
                                    label: `待审核${checkRecords.length > 0 ? ` (${checkRecords.length})` : ''}`,
                                    children: (
                                        <Table
                                            columns={checkColumns}
                                            dataSource={checkRecords as unknown as Record<string, unknown>[]}
                                            rowKey="FinanceID"
                                            loading={loading}
                                            emptyText="暂无待审核记录"
                                        />
                                    ),
                                } as const,
                            ]
                            : []),
                        // 所有人：我的捐赠
                        {
                            key: 'myDonations',
                            label: '我的捐赠',
                            children: (
                                <Table
                                    columns={donationColumns}
                                    dataSource={myDonations as unknown as Record<string, unknown>[]}
                                    rowKey="DonationID"
                                    loading={loading}
                                    emptyText="暂无捐赠记录"
                                />
                            ),
                        },
                    ]}
                />

                <CreateRecordDrawer
                    open={drawerOpen}
                    activeKey={activeKey}
                    onClose={() => setDrawerOpen(false)}
                    onCreateExpense={handleCreateExpense}
                    onCreateDonation={handleCreateDonation}
                />

                {isAdmin && (
                    <div className="finance-bottom-actions finance-records-actions">
                        <Button type="default" size="small" onClick={() => navigate('/finance/statistics')}>
                            <Icon name="icon-design" size={15} />
                            统计报表
                        </Button>
                    </div>
                )}
            </Card>
        </section>
    )
}
