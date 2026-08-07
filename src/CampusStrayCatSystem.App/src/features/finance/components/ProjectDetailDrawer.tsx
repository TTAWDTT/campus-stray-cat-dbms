import { useEffect, useState } from 'react'
import { Button, Card, Divider, Drawer, Icon, Table, type TableColumn, Tag } from 'animal-island-ui'
import { financeService, type FinancialDisclosureDetail, type DonationRecord, type ExpenseRecord } from '../../../services/finance.service'
import { buildMockDisclosureDetail } from '../test/mockData'
//本组件用于展示某个众筹项目的财务明细，包括核心指标、捐款明细和支出明细。它使用了一个抽屉组件来显示详细信息，并通过表格展示捐款和支出记录。
const fmt = (v: number | null | undefined): string => {
    if (v == null) return '-'
    return v.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const payMethodLabel: Record<string, string> = { ALIPAY: '支付宝', WECHAT: '微信', BANK_TRANSFER: '银行转账', CASH: '现金', OTHER: '其他' }
const recordTypeLabel: Record<string, string> = { FOOD: '食物', MEDICAL: '医疗', SUPPLIES: '物资', OTHER: '其他' }

const castDonation = (row: Record<string, unknown>) => row as unknown as DonationRecord
const castExpense = (row: Record<string, unknown>) => row as unknown as ExpenseRecord

type Props = {
    open: boolean
    projectId: string | null
    projectTitle?: string
    onClose: () => void
    useMock?: boolean
}

export function ProjectDetailDrawer({ open, projectId, projectTitle, onClose, useMock }: Props) {
    const [detail, setDetail] = useState<FinancialDisclosureDetail | null>(null)
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState('')

    useEffect(() => {
        if (open && projectId) {
            setLoading(true)
            setError('')
            const fetch = useMock
                ? Promise.resolve(buildMockDisclosureDetail(projectId))
                : financeService.getDisclosureDetail(projectId)
            fetch
                .then(setDetail)
                .catch((e) => setError(e instanceof Error ? e.message : '加载失败'))
                .finally(() => setLoading(false))
        }
    }, [open, projectId, useMock])

    const donationColumns: TableColumn[] = [
        { title: '捐赠人', width: 120, render: (_v, row) => {
            const d = castDonation(row)
            return <span>{d.PublicFlag === 1 ? d.DonorUserID || '-' : '匿名'}</span>
        }},
        { title: '金额', width: 100, render: (_v, row) => <span>¥{fmt(castDonation(row).Amount)}</span> },
        { title: '方式', width: 80, render: (_v, row) => <span>{payMethodLabel[castDonation(row).PayMethod] || '-'}</span> },
        { title: '时间', dataIndex: 'PayTime', width: 140, render: (_v, row) => {
            const t = castDonation(row).PayTime
            return <span>{t?.slice(0, 10) ?? '-'}</span>
        }},
    ]

    const expenseColumns: TableColumn[] = [
        { title: '类型', width: 80, render: (_v, row) => <span>{recordTypeLabel[castExpense(row).RecordType] || '-'}</span> },
        { title: '金额', width: 100, render: (_v, row) => <span>¥{fmt(castExpense(row).Amount)}</span> },
        { title: '状态', width: 80, render: (_v, row) => {
            const s = castExpense(row).AuditStatus || 'PENDING'
            const color = s === 'APPROVED' ? 'app-green' : s === 'REJECTED' ? 'app-red' : 'app-yellow'
            return <Tag color={color} variant="soft">{s === 'APPROVED' ? '已通过' : s === 'REJECTED' ? '已驳回' : '待审核'}</Tag>
        }},
        { title: '发票', width: 100, render: (_v, row) => {
            const url = castExpense(row).InvoiceURL
            return url ? <a href={url} target="_blank" rel="noreferrer" style={{ fontSize: 12 }}>查看</a> : <span>-</span>
        }},
        { title: '公开时间', dataIndex: 'PublicTime', width: 120, render: (_v, row) => {
            const t = castExpense(row).PublicTime
            return <span>{t?.slice(0, 10) ?? '-'}</span>
        }},
    ]

    return (
        <Drawer open={open} onClose={onClose} title={`项目明细${projectTitle ? ` — ${projectTitle}` : ''}`} width={640}>
            {loading && (
                <div style={{ padding: 40, textAlign: 'center' }}>
                    <Icon name="icon-camera" size={24} />
                    <p>加载中…</p>
                </div>
            )}
            {error && (
                <div className="cats-alert" role="alert">
                    <Icon name="icon-camera" size={17} />
                    <span>{error}</span>
                    <Button type="text" size="small" onClick={() => setError('')}>知道了</Button>
                </div>
            )}
            {!loading && !error && detail && (
                <>
                    {/* 核心指标 */}
                    <div className="detail-metrics">
                        <div className="detail-metric">
                            <span className="detail-metric-label">已筹金额</span>
                            <span className="detail-metric-value">¥{fmt(detail.RaisedAmount)}</span>
                        </div>
                        <div className="detail-metric">
                            <span className="detail-metric-label">已通过支出</span>
                            <span className="detail-metric-value">¥{fmt(detail.TotalExpense)}</span>
                        </div>
                        <div className="detail-metric">
                            <span className="detail-metric-label">净余额</span>
                            <span className={`detail-metric-value ${(detail.NetBalance ?? 0) >= 0 ? 'is-positive' : 'is-negative'}`}>
                                ¥{fmt(detail.NetBalance)}
                            </span>
                        </div>
                        <div className="detail-metric">
                            <span className="detail-metric-label">捐赠笔数</span>
                            <span className="detail-metric-value">{detail.DonationCount ?? 0}</span>
                        </div>
                    </div>

                    <Card style={{ marginBottom: 16 }}>
                        <div className="detail-section-header">
                            <h3>项目信息</h3>
                        </div>
                        <div className="detail-info-grid">
                            <div><span>目标金额</span><strong>¥{fmt(detail.TargetAmount)}</strong></div>
                            <div><span>开始时间</span><strong>{detail.Project.StartTime?.slice(0, 10) ?? '-'}</strong></div>
                            <div><span>结束时间</span><strong>{detail.Project.EndTime?.slice(0, 10) ?? '-'}</strong></div>
                            <div><span>关联猫咪</span><strong>{detail.Project.CatID || '无'}</strong></div>
                        </div>
                    </Card>

                    <Divider type="dashed-yellow" />

                    {/* 捐款明细 */}
                    <div className="detail-section">
                        <div className="detail-section-header">
                            <h3>捐款明细</h3>
                            <span className="detail-section-count">{detail.Donations.length} 笔</span>
                        </div>
                        {detail.Donations.length > 0 ? (
                            <Table
                                columns={donationColumns}
                                dataSource={detail.Donations as unknown as Record<string, unknown>[]}
                                rowKey="DonationID"
                                emptyText="暂无捐款记录"
                            />
                        ) : (
                            <p className="detail-empty">暂无捐款记录</p>
                        )}
                    </div>

                    <Divider type="dashed-yellow" />

                    {/* 支出明细 */}
                    <div className="detail-section">
                        <div className="detail-section-header">
                            <h3>支出明细</h3>
                            <span className="detail-section-count">{detail.Expenses.length} 笔</span>
                        </div>
                        {detail.Expenses.length > 0 ? (
                            <Table
                                columns={expenseColumns}
                                dataSource={detail.Expenses as unknown as Record<string, unknown>[]}
                                rowKey="FinanceID"
                                emptyText="暂无支出记录"
                            />
                        ) : (
                            <p className="detail-empty">暂无支出记录</p>
                        )}
                    </div>
                </>
            )}
            {!loading && !error && !detail && (
                <p style={{ textAlign: 'center', padding: 40, color: '#888' }}>暂无数据</p>
            )}
        </Drawer>
    )
}
