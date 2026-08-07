import { useNavigate } from 'react-router-dom'
import { Button, Icon } from 'animal-island-ui'
import { useEffect, useState } from 'react'
import { financeService } from '../../../services/finance.service'
import type { CrowdfundingProject } from '../../../services/finance.service'
import { mockProjects, USE_MOCK } from '../test/mockData'
import { FundCards } from '../components/FundCards'
import { ProjectPreviewList } from '../components/ProjectPreviewList'
import { PageHeader } from '../../../shared/components/PageHeader'
//本页面用于展示财务管理的概览，包括发起众筹项目和查看近期众筹项目的预览列表。用户可以通过点击“发起众筹”按钮创建新的众筹项目，或者点击“查看更多”按钮进入详细的项目列表页面。
export function FinancePage() {
    const navigate = useNavigate()
    const [projects, setProjects] = useState<CrowdfundingProject[]>([])

    useEffect(() => {
        if (USE_MOCK) {
            setProjects(mockProjects)
        } else {
            financeService.listProjects().then(setProjects)
        }
    }, [])

    return (
        <section className="feature-page finance-hub">
            <PageHeader kicker="FINANCE" title="财务管理" icon="icon-shopping" />

            <FundCards
                onProjectCreated={(project) => {
                    setProjects((prev) => [project, ...prev])
                }}
            />
            <ProjectPreviewList projects={projects} />
            <div className="finance-bottom-actions">
                <Button type="default" onClick={() => navigate('/finance/records')}>
                    <Icon name="icon-camera" size={15} />
                    查看记录
                </Button>
                <Button type="default" onClick={() => navigate('/finance/statistics')}>
                    <Icon name="icon-design" size={15} />
                    统计报表
                </Button>
            </div>
        </section>
    )
}
