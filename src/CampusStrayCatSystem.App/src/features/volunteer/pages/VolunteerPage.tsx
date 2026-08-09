import { Card, Icon, Button, Table, type TableColumn } from 'animal-island-ui'
import { useNavigate } from 'react-router-dom'
import type { KeyboardEvent } from 'react'
import { useEffect, useState } from 'react'
import { VolunteerService, shiftStatusLabels } from '../../../services/volunteer.service'
import { useAuthStore } from '../../../stores/auth.store'

export function VolunteerPage() {
    const navigate = useNavigate()
    const [pendingCount, setPendingCount] = useState(0)
    const [myShifts, setMyShifts] = useState<any[]>([])

    useEffect(() => {
        VolunteerService.getPendingApplications()
            .then((data) => setPendingCount(data.length))
            .catch(() => setPendingCount(0))
    }, [])

    useEffect(() => {
        const userId = useAuthStore.getState().user?.userId
        VolunteerService.getActivity().then((data) => {
            const filtered = userId
                ? data.filter((item: any) => item.volunteerId === userId)
                : data.slice(0, 3)
            setMyShifts(filtered)
        }).catch(() => setMyShifts([]))
    }, [])

    const activateCard = (action: () => void) => (e: KeyboardEvent<HTMLDivElement>) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault()
            action()
        }
    }

    const shiftColumns: TableColumn<any>[] = [
        { title: '排班编号', dataIndex: 'shiftId' },
        { title: '开始时间', dataIndex: 'planStartTime', render: (t: any) => t ? t.format('MM-DD HH:mm') : '-' },
        { title: '结束时间', dataIndex: 'planEndTime', render: (t: any) => t ? t.format('MM-DD HH:mm') : '-' },
        { title: '状态', dataIndex: 'shiftStatus', render: (t: any) => shiftStatusLabels[t] || t },
    ]

    return (
        <>
            <div className="finance-hero-grid">
                <div className="finance-hero-card-hit" role="button" tabIndex={0}
                    aria-label="领养审核"
                    onClick={() => navigate('/volunteer/adoptions')}
                    onKeyDown={activateCard(() => navigate('/volunteer/adoptions'))}>
                    <Card color="app-teal" className="finance-hero-card">
                        <div className="finance-hero-card-inner">
                            <span className="finance-hero-icon">
                                <Icon name="icon-chat" size={28} />
                            </span>
                            <h2>领养审核</h2>
                            <p>处理待审核的领养申请，仔细评估每一位申请人是否适合领养。</p>
                            {pendingCount > 0 && (
                                <span className="volunteer-pending-badge">
                                    <strong>{pendingCount}</strong>
                                    <small>条待审核</small>
                                </span>
                            )}
                        </div>
                    </Card>
                </div>

                <div className="finance-hero-card-hit" role="button" tabIndex={0}
                    aria-label="回访汇总"
                    onClick={() => navigate('/volunteer/visits')}
                    onKeyDown={activateCard(() => navigate('/volunteer/visits'))}>
                    <Card color="app-blue" className="finance-hero-card">
                        <div className="finance-hero-card-inner">
                            <span className="finance-hero-icon">
                                <Icon name="icon-camera" size={28} />
                            </span>
                            <h2>回访汇总</h2>
                            <p>查看已完成领养的回访记录，确保每一只猫咪在新家都过得幸福。</p>
                        </div>
                    </Card>
                </div>
            </div>

            {myShifts.length > 0 && (
                <div className="volunteer-my-shifts">
                    <h3>我的排班</h3>
                    <Table columns={shiftColumns} dataSource={myShifts} />
                    <div style={{ marginTop: 12 }}>
                        <Button type="primary" size="small" onClick={() => navigate('/volunteer/activity')}>
                            查看全部
                        </Button>
                    </div>
                </div>
            )}
        </>
    )
}