import { useNavigate } from 'react-router-dom'
import { Button, Card, Icon, Table, type TableColumn } from 'animal-island-ui'
import { useEffect, useState } from 'react'
import { StatusTag } from '../../../shared/components/StatusTag'
import { financeService } from '../../../services/finance.service'
import type { CrowdfundingProject, UpdateProjectPayload } from '../../../services/finance.service'
import { mockProjects } from '../test/mockData'
import { ProjectEditDrawer } from '../components/ProjectEditDrawer'
import { CreateRecordDrawer } from '../components/CreateRecordDrawer'
import { ProjectDetailDrawer } from '../components/ProjectDetailDrawer'
import type { CreateDonationPayload } from '../../../services/finance.service'
import { useAuthStore } from '../../../stores/auth.store'
//本页面用于展示所有众筹项目的列表，并提供编辑和捐款功能。用户可以查看项目的详细信息，编辑项目信息，或者为进行中的项目捐款。
const USE_MOCK = true

const statusOrder: Record<string, number> = {
    ACTIVE: 0,
    CANCELLED: 1,
    COMPLETED: 2,
}

const sortByStatus = (list: CrowdfundingProject[]) =>
    [...list].sort((a, b) => {
        const orderA = statusOrder[a.ProjectStatus] ?? 99
        const orderB = statusOrder[b.ProjectStatus] ?? 99
        return orderA - orderB
    })

const statusLabel: Record<string, string> = {
    ACTIVE: '进行中',
    COMPLETED: '已结束',
    CANCELLED: '已取消',
}

const castProject = (row: Record<string, unknown>) => row as unknown as CrowdfundingProject

export function ProjectPage() {
    const navigate = useNavigate()
    const user = useAuthStore((s) => s.user)
    const isAdmin = user?.roleName?.toUpperCase() === 'ADMIN'
    const [projects, setProjects] = useState<CrowdfundingProject[]>([])
    const [drawerOpen, setDrawerOpen] = useState(false)
    const [editingProject, setEditingProject] = useState<CrowdfundingProject | null>(null)
    const [donationOpen, setDonationOpen] = useState(false)
    const [donationTarget, setDonationTarget] = useState<CrowdfundingProject | null>(null)
    const [detailOpen, setDetailOpen] = useState(false)
    const [detailTarget, setDetailTarget] = useState<CrowdfundingProject | null>(null)

    const loadProjects = () => {
        if (USE_MOCK) {
            setProjects(sortByStatus(mockProjects))
        } else {
            financeService.listProjects().then((data) => setProjects(sortByStatus(data)))
        }
    }

    useEffect(() => { loadProjects() }, [])

    const openEdit = (project: CrowdfundingProject) => {
        setEditingProject(project)
        setDrawerOpen(true)
    }

    const closeEdit = () => {
        setDrawerOpen(false)
        setEditingProject(null)
    }

    const openDonation = (project: CrowdfundingProject) => {
        setDonationTarget(project)
        setDonationOpen(true)
    }

    const closeDonation = () => {
        setDonationOpen(false)
        setDonationTarget(null)
    }

    const openDetail = (project: CrowdfundingProject) => {
        setDetailTarget(project)
        setDetailOpen(true)
    }

    const closeDetail = () => {
        setDetailOpen(false)
        setDetailTarget(null)
    }

    const handleCreateDonation = async (payload: CreateDonationPayload) => {
        if (USE_MOCK) {
            console.log('捐款记录已创建（mock）:', payload)
        } else {
            await financeService.createDonation(payload)
        }
    }

    const handleUpdate = async (id: string, payload: UpdateProjectPayload) => {
        if (USE_MOCK) {
            setProjects((prev) =>
                sortByStatus(
                    prev.map((p) =>
                        p.ProjectID === id
                            ? { ...p, ...payload, ProjectID: p.ProjectID, RaisedAmount: p.RaisedAmount }
                            : p,
                    ),
                ),
            )
        } else {
            await financeService.updateProject(id, payload)
            loadProjects()
        }
    }

    const projectColumns: TableColumn[] = [
        {
            title: '项目名称',
            dataIndex: 'Title',
            render: (_value, row) => {
                const project = castProject(row)
                return <strong className="finance-project-table-name">{project.Title || '未命名项目'}</strong>
            },
        },
        {
            title: '目标金额',
            width: 130,
            render: (_value, row) => {
                const project = castProject(row)
                return <span>¥{project.TargetAmount?.toLocaleString() ?? '-'}</span>
            },
        },
        {
            title: '已筹金额',
            width: 130,
            render: (_value, row) => {
                const project = castProject(row)
                return <span>¥{project.RaisedAmount?.toLocaleString() ?? '0'}</span>
            },
        },
        {
            title: '开始时间',
            width: 130,
            dataIndex: 'StartTime',
        },
        {
            title: '结束时间',
            width: 130,
            dataIndex: 'EndTime',
        },
        {
            title: '状态',
            width: 110,
            render: (_value, row) => {
                const project = castProject(row)
                const value = project.ProjectStatus || 'ACTIVE'
                return <StatusTag value={value} label={statusLabel[value] || value} />
            },
        },
        {
            title: '操作',
            width: 80,
            render: (_value, row) => {
                const project = castProject(row)
                return (
                    <div style={{ display: 'flex', gap: 8 }}>
                        {isAdmin && (
                            <Button type="default" size="small" onClick={() => openEdit(project)}>
                                编辑
                            </Button>
                        )}
                        <Button type="default" size="small" onClick={() => openDetail(project)}>
                            明细
                        </Button>
                        {project.ProjectStatus === 'ACTIVE' && (
                            <Button type="default" size="small" onClick={() => openDonation(project)}>
                                捐款
                            </Button>
                        )}
                    </div>
                )
            },
        },
    ]

    const data = projects ?? []

    return (
        <section className="feature-page finance-project-page">
            <div className="feature-page-header">
                <div className="feature-page-heading">
                    <Button type="text" size="small" onClick={() => navigate('/finance')}>
                        <Icon name="icon-miles" size={15} />
                        <span style={{ marginLeft: 4 }}>返回</span>
                    </Button>
                    <div className="feature-page-title-row" style={{ marginTop: 12 }}>
                        <span className="feature-page-icon">
                            <Icon name="icon-shopping" size={21} />
                        </span>
                        <p className="kicker">FINANCE · PROJECTS</p>
                    </div>
                    <h1>众筹项目</h1>
                    <p>所有校园猫咪相关的众筹项目，点击编辑修改项目信息。</p>
                </div>
            </div>
            {data.length > 0 ? (
                <Card className="cats-table-card">
                    <div className="cats-table-page">
                        <Table
                            columns={projectColumns}
                            dataSource={data as unknown as Record<string, unknown>[]}
                            rowKey="ProjectID"
                            emptyText="暂无众筹项目"
                        />
                    </div>
                </Card>
            ) : (
                <Card className="feature-empty-state">
                    <span className="feature-empty-icon">
                        <Icon name="icon-shopping" size={28} />
                    </span>
                    <h2>还没有众筹项目</h2>
                    <p>去发起第一个为校园猫咪提供帮助的众筹项目吧，每一份支持都会被清晰记录。</p>
                    <Button type="primary" onClick={() => navigate('/finance')}>
                        返回财务管理
                    </Button>
                </Card>
            )}
            <ProjectEditDrawer
                open={drawerOpen}
                project={editingProject}
                onClose={closeEdit}
                onSubmit={handleUpdate}
            />
            <CreateRecordDrawer
                open={donationOpen}
                activeKey="donation"
                onClose={closeDonation}
                onCreateExpense={async () => {}}
                onCreateDonation={handleCreateDonation}
                lockedProjectID={donationTarget?.ProjectID}
                lockedDonorUserID={user?.userId}
            />
            <ProjectDetailDrawer
                open={detailOpen}
                projectId={detailTarget?.ProjectID ?? null}
                projectTitle={detailTarget?.Title}
                onClose={closeDetail}
                useMock={USE_MOCK}
            />
        </section>
    )
}
