import { Card, Divider, Icon } from 'animal-island-ui'
import { StatusTag } from '../../../shared/components/StatusTag'
import type { FinancialDisclosureSummary } from '../../../services/finance.service'
//本组件用于展示财务公示信息
type SummaryCardProps = {
    data: FinancialDisclosureSummary[]
    loading?: boolean
}

const projectStatusLabel: Record<string, string> = {
    ACTIVE: '进行中',
    COMPLETED: '已结束',
    CANCELLED: '已取消',
}

//格式化金额，始终保留两位小数
const fmt = (v: number | null | undefined): string => {
    if (v == null) return '-'
    return v.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export function FinanceSummaryCard({ data, loading }: SummaryCardProps) {
    if (loading) {
        return (
            <Card className="finance-summary-card">
                <div className="finance-summary-loading">
                    <Icon name="icon-camera" size={21} />
                    <span>正在加载公示数据…</span>
                </div>
            </Card>
        )
    }

    if (!data || data.length === 0) {
        return (
            <Card className="finance-summary-card">
                <div className="finance-summary-empty">
                    <Icon name="icon-camera" size={28} />
                    <p>暂无进行中的众筹项目</p>
                </div>
            </Card>
        )
    }

    return (
        <Card className="finance-summary-card">
            <div className="finance-summary-header">
                <h2>财务公示</h2>
                <p>以下为所有进行中项目的实时财务汇总，资金流向公开透明。</p>
            </div>

            <div className="finance-summary-grid">
                {data.map((item, index) => {
                    const project = item.Project
                    const progress = item.TargetAmount && item.TargetAmount > 0
                        ? Math.min(100, Math.round(((item.RaisedAmount ?? 0) / item.TargetAmount) * 100))
                        : 0

                    return (
                        <div key={project.ProjectID || index} className="finance-summary-item">
                            {index > 0 && <Divider type="dashed-yellow" />}

                            {/* 项目标题行 */}
                            <div className="fsi-header">
                                <span className="fsi-project-name">{project.Title || '未命名项目'}</span>
                                <StatusTag value={project.ProjectStatus} label={projectStatusLabel[project.ProjectStatus] || project.ProjectStatus} />
                            </div>

                            {/* 金额主卡片 — 三列核心数字 */}
                            <div className="fsi-metrics">
                                <div className="fsi-metric fsi-metric--raised">
                                    <span className="fsi-metric-label">已筹金额</span>
                                    <span className="fsi-metric-value">¥{fmt(item.RaisedAmount)}</span>
                                </div>
                                <div className="fsi-metric fsi-metric--expense">
                                    <span className="fsi-metric-label">已通过支出</span>
                                    <span className="fsi-metric-value">¥{fmt(item.TotalExpense)}</span>
                                </div>
                                <div className="fsi-metric fsi-metric--balance">
                                    <span className="fsi-metric-label">净余额</span>
                                    <span className="fsi-metric-value">¥{fmt(item.NetBalance)}</span>
                                </div>
                            </div>

                            {/* 辅助信息行 */}
                            <div className="fsi-aux">
                                <span>目标金额 <strong>¥{fmt(item.TargetAmount)}</strong></span>
                                <span>·</span>
                                <span>捐赠笔数 <strong>{item.DonationCount ?? 0}</strong></span>
                                {item.TargetAmount && item.TargetAmount > 0 && (
                                    <>
                                        <span>·</span>
                                        <span>达成率 <strong>{progress}%</strong></span>
                                    </>
                                )}
                            </div>

                            {/* 进度条 */}
                            {item.TargetAmount && item.TargetAmount > 0 && (
                                <div className="fsi-progress-track">
                                    <div
                                        className="fsi-progress-fill"
                                        style={{ width: `${progress}%` }}
                                    />
                                </div>
                            )}
                        </div>
                    )
                })}
            </div>
        </Card>
    )
}
