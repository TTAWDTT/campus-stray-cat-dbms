import { useNavigate } from 'react-router-dom'
import { Button, Card, Divider, Icon } from 'animal-island-ui'
import { StatusTag } from '../../../shared/components/StatusTag'
import type { CrowdfundingProject } from '../../../services/finance.service'
//本组件用于展示众筹项目的预览列表，最多显示四个项目，并提供查看更多的按钮。
const PREVIEW_COUNT = 4

const statusLabel: Record<string, string> = {
    ACTIVE: '进行中',
    COMPLETED: '已结束',
    CANCELLED: '已取消',
}

type Props = {
    projects: CrowdfundingProject[]
}

export function ProjectPreviewList({ projects }: Props) {
    const navigate = useNavigate()
    const preview = projects.slice(0, PREVIEW_COUNT)

    return (
        <Card className="finance-project-list-card">
            <div className="section-heading">
                <div>
                    <h2>项目清单</h2>
                    <p className="finance-project-subtitle">
                        {preview.length > 0
                            ? `近期共 ${projects.length} 个项目`
                            : '还没有众筹项目'}
                    </p>
                </div>
            </div>

            {preview.length > 0 ? (
                <div className="finance-project-items">
                    {preview.map((item, index) => (
                        <div key={item.ProjectID || index}>
                            {index > 0 && <Divider type="dashed-yellow" />}
                            <div className="finance-project-row">
                                <span className="finance-project-title">{item.Title}</span>
                                <span className="finance-project-meta">
                                    目标 ¥{item.TargetAmount?.toLocaleString() ?? '-'}
                                    <em>·</em>
                                    已筹 ¥{item.RaisedAmount?.toLocaleString() ?? '0'}
                                </span>
                                <StatusTag value={item.ProjectStatus} label={statusLabel[item.ProjectStatus] || item.ProjectStatus} />
                            </div>
                        </div>
                    ))}
                </div>
            ) : (
                <div className="finance-empty-projects">
                    <Icon name="icon-camera" size={32} />
                    <p>暂无众筹项目</p>
                    <small>点击「发起众筹」创建第一个项目</small>
                </div>
            )}

            <div className="finance-project-actions">
                <Button type="default" onClick={() => navigate('/finance/projects')}>
                    查看更多
                </Button>
            </div>
        </Card>
    )
}
