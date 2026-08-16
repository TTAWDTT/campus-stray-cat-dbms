import { Card, Icon, Modal } from 'animal-island-ui'
import { useEffect, useState } from 'react'
import { financeService } from '../../../services/finance.service'
import type { CrowdfundingProject } from '../../../services/finance.service'
import { mockProjects, USE_MOCK } from '../test/mockData'
import { FundCards } from '../components/FundCards'
import { ProjectPreviewList } from '../components/ProjectPreviewList'
import { PageHeader } from '../../../shared/components/PageHeader'
import { RecordsPage } from './RecordsPage'
import { StatisticsPage } from './StatisticsPage'
//本页面用于展示财务管理的概览，包括发起众筹项目和查看近期众筹项目的预览列表。用户可以通过点击“发起众筹”按钮创建新的众筹项目，或者点击“查看更多”按钮进入详细的项目列表页面。
export function FinancePage() {
    const [projects, setProjects] = useState<CrowdfundingProject[]>([])
    const [detail, setDetail] = useState<'records' | 'statistics' | null>(null)

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

            <div className="finance-content-grid">
                <div className="finance-primary-stack"><FundCards onProjectCreated={(project) => setProjects((prev) => [project, ...prev])} /></div>
                <div className="finance-side-actions">
                    <div className="finance-side-card-hit" role="button" tabIndex={0} onClick={() => setDetail('records')} onKeyDown={(event) => { if (event.key === 'Enter' || event.key === ' ') setDetail('records') }}>
                        <Card hoverable color="app-yellow" className="finance-side-card"><Icon name="icon-camera" size={22} /><span><strong>查看财务记录</strong><small>捐款、支出与审核流水</small></span></Card>
                    </div>
                    <div className="finance-side-card-hit" role="button" tabIndex={0} onClick={() => setDetail('statistics')} onKeyDown={(event) => { if (event.key === 'Enter' || event.key === ' ') setDetail('statistics') }}>
                        <Card hoverable color="app-orange" className="finance-side-card"><Icon name="icon-design" size={22} /><span><strong>统计报表</strong><small>按项目查看财务指标快照</small></span></Card>
                    </div>
                </div>
            </div>
            <ProjectPreviewList projects={projects} />
            <Modal open={detail === 'records'} title="财务记录" width={1040} typewriter={false} onClose={() => setDetail(null)}><RecordsPage embedded /></Modal>
            <Modal open={detail === 'statistics'} title="统计报表" width={1040} typewriter={false} onClose={() => setDetail(null)}><StatisticsPage embedded /></Modal>
        </section>
    )
}
