import { useNavigate } from 'react-router-dom'
import { Button, Card, Icon, Table, type TableColumn, Tag } from 'animal-island-ui'
import { useEffect, useMemo, useState } from 'react'
import { financeService, type SnapshotRecord, metricLabels, dimensionLabels } from '../../../services/finance.service'
import { mockSnapshots, USE_MOCK } from '../test/mockData'
import { useAuthStore } from '../../../stores/auth.store'

const castSnapshot = (row: Record<string, unknown>) => row as unknown as SnapshotRecord

// 格式化金额
const fmt = (code: string, val: number | null): string => {
    if (val == null) return '-'
    if (code === 'DONATION_COUNT') return String(val)
    return `¥${val.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

const exportSnapshotsToExcel = (snapshots: SnapshotRecord[]) => {
    const escape = (value: unknown) => String(value ?? '—').replace(/[&<>]/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;' })[char] || char)
    const rows = snapshots.map((item) => `<tr><td>${escape(item.SnapshotID)}</td><td>${escape(item.SnapshotDate?.slice(0, 10))}</td><td>${escape(metricLabels[item.MetricCode] || item.MetricCode)}</td><td>${escape(fmt(item.MetricCode, item.MetricValue))}</td><td>${escape(dimensionLabels[item.DimensionType || ''] || item.DimensionType)}</td><td>${escape(item.DimensionValue)}</td></tr>`).join('')
    const html = `<table><thead><tr><th>快照 ID</th><th>日期</th><th>指标</th><th>数值</th><th>维度</th><th>维度值</th></tr></thead><tbody>${rows}</tbody></table>`
    const url = URL.createObjectURL(new Blob([`<html><meta charset="utf-8">${html}</html>`], { type: 'application/vnd.ms-excel' }))
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = '财务统计报表.xls'
    anchor.click()
    URL.revokeObjectURL(url)
}

export function StatisticsPage({ embedded = false }: { embedded?: boolean }) {
    const navigate = useNavigate()
    const user = useAuthStore((s) => s.user)
    const isAdmin = user?.roleName?.toUpperCase() === 'ADMIN'

    const [snapshots, setSnapshots] = useState<SnapshotRecord[]>([])
    const [loading, setLoading] = useState(true)
    const [generating, setGenerating] = useState(false)
    const [error, setError] = useState('')
    const [message, setMessage] = useState('')

    const loadData = async () => {
        setLoading(true)
        setError('')
        try {
            if (USE_MOCK) {
                await new Promise((r) => setTimeout(r, 300))
                setSnapshots(mockSnapshots)
            } else {
                const data = await financeService.listSnapshots()
                setSnapshots(data)
            }
        } catch {
            setError('加载快照数据失败')
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => { loadData() }, [])

    // 按维度-指标分组，便于卡片展示
    const grouped = useMemo(() => {
        const map = new Map<string, SnapshotRecord[]>()
        for (const s of snapshots) {
            const key = `${s.DimensionType ?? ''}|${s.DimensionValue ?? ''}`
            const list = map.get(key) || []
            list.push(s)
            map.set(key, list)
        }
        return Array.from(map.entries())
    }, [snapshots])

    // 生成快照（管理员）
    const handleGenerate = async () => {
        const projectId = window.prompt('请输入要生成统计快照的项目 ID：')
        if (!projectId) return
        setGenerating(true)
        setMessage('')
        setError('')
        try {
            const result = await financeService.generateProjectReport(projectId.trim())
            setMessage(result.message || '统计报表已生成')
            await loadData()
        } catch (e: unknown) {
            const msg = e instanceof Error ? e.message : '生成失败'
            setError(msg)
        } finally {
            setGenerating(false)
        }
    }

    const columns: TableColumn[] = [
        {
            title: '快照ID',
            dataIndex: 'SnapshotID',
            width: 140,
        },
        {
            title: '快照日期',
            dataIndex: 'SnapshotDate',
            width: 120,
            render: (_v, row) => {
                const s = castSnapshot(row)
                return <span>{s.SnapshotDate?.slice(0, 10) ?? '-'}</span>
            },
        },
        {
            title: '指标',
            dataIndex: 'MetricCode',
            width: 100,
            render: (_v, row) => {
                const s = castSnapshot(row)
                return <span>{metricLabels[s.MetricCode] || s.MetricCode}</span>
            },
        },
        {
            title: '数值',
            dataIndex: 'MetricValue',
            width: 130,
            render: (_v, row) => {
                const s = castSnapshot(row)
                const color = s.MetricCode === 'NET_BALANCE'
                    ? (s.MetricValue ?? 0) >= 0 ? 'app-green' : 'app-red'
                    : undefined
                return color
                    ? <Tag color={color} variant="soft">{fmt(s.MetricCode, s.MetricValue)}</Tag>
                    : <strong>{fmt(s.MetricCode, s.MetricValue)}</strong>
            },
        },
        {
            title: '维度类型',
            dataIndex: 'DimensionType',
            width: 90,
            render: (_v, row) => {
                const s = castSnapshot(row)
                return <span>{dimensionLabels[s.DimensionType ?? ''] || s.DimensionType || '-'}</span>
            },
        },
        {
            title: '维度值',
            dataIndex: 'DimensionValue',
            width: 130,
        },
        {
            title: '单位',
            dataIndex: 'Unit',
            width: 70,
            render: (_v, row) => {
                const s = castSnapshot(row)
                return <span>{s.Unit === 'CNY' ? '元' : s.Unit === 'COUNT' ? '次' : s.Unit || '-'}</span>
            },
        },
        {
            title: '生成时间',
            dataIndex: 'GenerateTime',
            width: 160,
            render: (_v, row) => {
                const s = castSnapshot(row)
                return <span>{s.GenerateTime?.slice(0, 19)?.replace('T', ' ') ?? '-'}</span>
            },
        },
    ]

    return (
        <section className={`feature-page finance-statistics-page${embedded ? ' finance-embedded-page' : ''}`}>
            {embedded ? <div className="finance-embedded-toolbar"><div><strong>统计报表</strong><small>按项目、月份和猫咪维度汇总</small></div><div><Button type="default" size="small" onClick={() => window.print()}>导出 PDF</Button><Button type="default" size="small" onClick={() => exportSnapshotsToExcel(snapshots)}>导出 Excel</Button></div></div> : <div className="feature-page-header">
                <div className="feature-page-heading">
                    <Button type="text" size="small" onClick={() => navigate('/finance')}>
                        <Icon name="icon-miles" size={15} />
                        <span style={{ marginLeft: 4 }}>返回</span>
                    </Button>
                    <div className="feature-page-title-row" style={{ marginTop: 12 }}>
                        <span className="feature-page-icon">
                            <Icon name="icon-design" size={21} />
                        </span>
                        <p className="kicker">FINANCE · STATISTICS</p>
                    </div>
                    <h1>统计报表</h1>
                    <p>基于预生成快照的财务指标存档，可按项目、月份、猫咪维度查看历史趋势。</p>
                </div>
            </div>}

            {error && (
                <div className="cats-alert" role="alert">
                    <Icon name="icon-camera" size={17} />
                    <span>{error}</span>
                    <Button type="text" size="small" onClick={() => setError('')}>知道了</Button>
                </div>
            )}
            {message && (
                <div className="cats-alert" role="alert" style={{ borderColor: 'var(--app-green)' }}>
                    <Icon name="icon-design" size={17} />
                    <span>{message}</span>
                    <Button type="text" size="small" onClick={() => setMessage('')}>知道了</Button>
                </div>
            )}

            {/* 按维度卡片摘要 */}
            {grouped.length > 0 && (
                <div className="snapshot-group-grid">
                    {grouped.map(([key, items]) => {
                        const [dimType, dimVal] = key.split('|')
                        const dimLabel = dimensionLabels[dimType] || dimType
                        return (
                            <Card key={key} className="snapshot-group-card">
                                <div className="snapshot-group-header">
                                    <span className="snapshot-group-dim">{dimLabel}</span>
                                    <strong>{dimVal}</strong>
                                </div>
                                <div className="snapshot-group-metrics">
                                    {items.map((item) => (
                                        <div key={item.SnapshotID} className="snapshot-group-metric">
                                            <span className="snapshot-metric-label">{metricLabels[item.MetricCode] || item.MetricCode}</span>
                                            <span className={`snapshot-metric-value${item.MetricCode === 'NET_BALANCE' ? ((item.MetricValue ?? 0) >= 0 ? ' is-positive' : ' is-negative') : ''}`}>
                                                {fmt(item.MetricCode, item.MetricValue)}
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            </Card>
                        )
                    })}
                </div>
            )}

            {/* 表格明细 */}
            <Card className="cats-table-card">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                    <h2>快照明细</h2>
                    {isAdmin && (
                        <Button type="primary" size="small" onClick={handleGenerate} loading={generating}>
                            + 生成快照
                        </Button>
                    )}
                </div>
                <Table
                    columns={columns}
                    dataSource={snapshots as unknown as Record<string, unknown>[]}
                    rowKey="SnapshotID"
                    loading={loading}
                    emptyText="暂无统计快照"
                />
            </Card>
        </section>
    )
}
