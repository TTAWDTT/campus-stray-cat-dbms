import { Card, Icon, Button, Table, type TableColumn, Drawer, Form, FormItem, useForm, Input, Notification ,Radio} from 'animal-island-ui'
import { useNavigate } from 'react-router-dom'
import type { KeyboardEvent } from 'react'
import { useEffect, useState } from 'react'
import { VolunteerService, shiftStatusLabels } from '../../../services/volunteer.service'
import { useAuthStore } from '../../../stores/auth.store'
import { StatusTag } from '../../../shared/components/StatusTag'
import { DatePicker } from 'antd'

export function VolunteerPage() {
    const navigate = useNavigate()
    const [pendingCount, setPendingCount] = useState(0)
    const [myShifts, setMyShifts] = useState<any[]>([])
    const [recruitOpen, setRecruitOpen] = useState(false)
    const [creditLogOpen, setCreditLogOpen] = useState(false)
    const [recruitForm] = useForm()
    const [creditForm] = useForm()
    const [feedingTasks, setFeedingTasks] = useState<any[]>([])
    const [feedingFilter, setFeedingFilter] = useState('all')
    const [feedingInput, setFeedingInput] = useState('')
    const [statusFilter, setStatusFilter] = useState('')
    const isAdmin = (useAuthStore.getState().user?.roleName?.toUpperCase() === 'ADMIN') ||true
    useEffect(() => {
        VolunteerService.getPendingApplications()
            .then((data) => setPendingCount(data.length))
            .catch(() => setPendingCount(0))
    }, [])

    useEffect(() => {
        const userId = useAuthStore.getState().user?.userId
        VolunteerService.getActivity().then((data) => {
            const filtered = userId
                ? data.filter((item: any) => item.userId === userId)
                : data.slice(0, 3)
            setMyShifts(filtered)
        }).catch(() => setMyShifts([]))
    }, [])

    useEffect(() => {
        VolunteerService.getAllFeedingTasks().then(setFeedingTasks).catch(() => setFeedingTasks([]))
    }, [])

    const queryFeedingTasks = async () => {
        try {
            if (feedingFilter === 'all') {
                const data = await VolunteerService.getAllFeedingTasks()
                setFeedingTasks(data)
            } else if (feedingFilter === 'id') {
                if (!feedingInput) { Notification.error('请输入任务ID'); return }
                const data = await VolunteerService.getFeedingTasksById(feedingInput)
                setFeedingTasks(data)
            } else if (feedingFilter === 'point') {
                if (!feedingInput) { Notification.error('请输入点位ID'); return }
                const data = await VolunteerService.getFeedingTasksByPoint(feedingInput)
                setFeedingTasks(data)
            } else if (feedingFilter === 'status') {
                if (!statusFilter) { Notification.error('请选择状态'); return }
                const data = await VolunteerService.getFeedingTasksByStatus(statusFilter)
                setFeedingTasks(data)
            }
        } catch {
            Notification.error('查询失败')
        }
    }

    const handleRecruit = async () => {
        const values = recruitForm.getFieldsValue()
        if (!values.userId) {
            Notification.error('用户ID不能为空')
            return
        }
        const payload: Record<string, unknown> = {
            userId: values.userId,
        }
        if (values.joinDate) {
            payload.joinDate = (values.joinDate as any).toISOString?.() ?? values.joinDate
        }
        if (values.serviceScore) payload.serviceScore = Number(values.serviceScore)
        if (values.creditLevel) payload.creditLevel = values.creditLevel
        if (values.activeStatus) payload.activeStatus = values.activeStatus
        if (values.graduationYear) payload.graduationYear = values.graduationYear

        try {
            await VolunteerService.registerVolunteer(payload)
            Notification.success('志愿者招募成功')
            setRecruitOpen(false)
            recruitForm.resetFields()
        } catch {
            Notification.error('招募失败')
        }
    }

    const handleCreditLog = async () => {
        const values = creditForm.getFieldsValue()
        if (!values.volunteerId || !values.sourceType || !values.sourceId || !values.scoreChange || !values.creditLevelAfter) {
            Notification.error('必填字段不能为空')
            return
        }
        const payload: Record<string, unknown> = {
            volunteerId: values.volunteerId,
            sourceType: values.sourceType,
            sourceId: values.sourceId || '',
            scoreChange: Number(values.scoreChange) || 0,
            creditLevelAfter: values.creditLevelAfter || '',
        }
        if (values.createTime) {
            payload.createTime = (values.createTime as any).toISOString?.() ?? values.createTime
        }
        if (values.remark) payload.remark = values.remark

        try {
            await VolunteerService.addCreditLog(payload)
            Notification.success('积分日志添加成功')
            setCreditLogOpen(false)
            creditForm.resetFields()
        } catch {
            Notification.error('添加失败')
        }
    }

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

    const feedingColumns: TableColumn<any>[] = [
        { title: '任务ID', dataIndex: 'shiftID' },
        { title: '志愿者', dataIndex: 'volunteerID' },
        { title: '点位', dataIndex: 'pointID' },
        { title: '开始时间', dataIndex: 'planStartTime', render: (t: any) => t ? t.format('MM-DD HH:mm') : '-' },
        { title: '结束时间', dataIndex: 'planEndTime', render: (t: any) => t ? t.format('MM-DD HH:mm') : '-' },
        {
            title: '状态', dataIndex: 'shiftStatus',
            render: (t: any) => {
                const map: Record<string, { value: string; label: string }> = {
                    PLANNED: { value: 'PENDING', label: '已排班' },
                    ASSIGNED: { value: 'PROCESSING', label: '已分配' },
                    IN_PROGRESS: { value: 'ACTIVE', label: '执行中' },
                    COMPLETED: { value: 'COMPLETED', label: '已完成' },
                    MISSED: { value: 'REJECTED', label: '逾期' },
                }
                const item = map[t]
                return item ? <StatusTag value={item.value} label={item.label} /> : <span>{t || '-'}</span>
            },
        },
    ]

    return (
        <>
            <div className="finance-hero-grid" style={{ gridTemplateColumns: isAdmin ? 'repeat(4, 1fr)' : 'repeat(2, 1fr)' }}>
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

                {isAdmin && (
                    <div className="finance-hero-card-hit" role="button" tabIndex={0}
                        aria-label="志愿者招募"
                        onClick={() => setRecruitOpen(true)}
                        onKeyDown={activateCard(() => setRecruitOpen(true))}>
                        <Card color="app-green" className="finance-hero-card">
                            <div className="finance-hero-card-inner">
                                <span className="finance-hero-icon">
                                    <Icon name="icon-diy" size={28} />
                                </span>
                                <h2>志愿者招募</h2>
                                <p>注册新的志愿者，让更多爱猫的同学加入到校园流浪猫的照护工作中来。</p>
                            </div>
                        </Card>
                    </div>
                )}

                {isAdmin && (
                    <div className="finance-hero-card-hit" role="button" tabIndex={0}
                        aria-label="新增积分日志"
                        onClick={() => setCreditLogOpen(true)}
                        onKeyDown={activateCard(() => setCreditLogOpen(true))}>
                        <Card color="app-yellow" className="finance-hero-card">
                            <div className="finance-hero-card-inner">
                                <span className="finance-hero-icon">
                                    <Icon name="icon-miles" size={28} />
                                </span>
                                <h2>新增积分日志</h2>
                                <p>为志愿者记录积分变动，更新信用等级，完整追踪每一次服务贡献。</p>
                            </div>
                        </Card>
                    </div>
                )}
            </div>


            <Drawer open={recruitOpen} onClose={() => { setRecruitOpen(false); recruitForm.resetFields() }} title="志愿者招募" >
                <Form form={recruitForm} layout='vertical'>
                    <FormItem label='用户ID' name='userId' required>
                        <Input placeholder='请输入用户ID' />
                    </FormItem>
                    <FormItem label='加入日期' name='joinDate'>
                        <DatePicker />
                    </FormItem>
                    <FormItem label='服务积分' name='serviceScore'>
                        <Input placeholder='默认 0' />
                    </FormItem>
                    <FormItem label='信用等级' name='creditLevel'>
                        <Radio options={[
                            { label: 'L1', value: 'L1' },
                            { label: 'L2', value: 'L2' },
                            { label: 'L3', value: 'L3' },
                        ]} />
                    </FormItem>
                    <FormItem label='在岗状态' name='activeStatus'>
                        <Radio options={[
                            { label: '在岗', value: 'ACTIVE' },
                            { label: '离岗', value: 'INACTIVE' },
                        ]} />
                    </FormItem>
                    <FormItem label='毕业年份' name='graduationYear'>
                        <Input placeholder='请输入毕业年份（选填）' />
                    </FormItem>
                    <FormItem>
                        <Button type='primary' style={{ marginRight: 8 }} onClick={handleRecruit}>提交</Button>
                        <Button type='default' onClick={() => { setRecruitOpen(false); recruitForm.resetFields() }}>取消</Button>
                    </FormItem>
                </Form>
            </Drawer>

            <Drawer open={creditLogOpen} onClose={() => { setCreditLogOpen(false); creditForm.resetFields() }} title="新增积分日志" >
                <Form form={creditForm} layout='vertical'>
                    <FormItem label='志愿者ID' name='volunteerId' required>
                        <Input placeholder='请输入志愿者ID' />
                    </FormItem>
                    <FormItem label='来源类型' name='sourceType' required>
                        <Radio options={[
                            { label: '排班', value: 'SHIFT' },
                            { label: '签到', value: 'CHECKIN' },
                            { label: '投喂', value: 'FEEDING' },
                            { label: '手动调整', value: 'MANUAL' },
                        ]} />
                    </FormItem>
                    <FormItem label='来源ID' name='sourceId' required>
                        <Input placeholder='请输入来源ID' />
                    </FormItem>
                    <FormItem label='积分变动' name='scoreChange' required>
                        <Input placeholder='请输入积分变动值' />
                    </FormItem>
                    <FormItem label='变动后信用等级' name='creditLevelAfter' required>
                        <Radio options={[
                            { label: 'L1', value: 'L1' },
                            { label: 'L2', value: 'L2' },
                            { label: 'L3', value: 'L3' },
                        ]} />
                    </FormItem>
                    <FormItem label='记录时间' name='createTime'>
                        <DatePicker showTime />
                    </FormItem>
                    <FormItem label='备注' name='remark'>
                        <Input placeholder='请输入备注（选填）' />
                    </FormItem>
                    <FormItem>
                        <Button type='primary' style={{ marginRight: 8 }} onClick={handleCreditLog}>提交</Button>
                        <Button type='default' onClick={() => { setCreditLogOpen(false); creditForm.resetFields() }}>取消</Button>
                    </FormItem>
                </Form>
            </Drawer>

            <div style={{ display: 'flex', gap: 20, marginTop: 20 }}>
                <div style={{ flex: '0 0 360px' }}>
                    <h3 style={{ margin: '0 0 12px 0' }}>我的排班</h3>
                    {myShifts.length > 0 ? (
                        <div className="volunteer-my-shifts">
                            <div style={{ maxHeight: 132, overflowY: 'auto' }}>
                                <Table columns={shiftColumns} dataSource={myShifts} />
                            </div>
                            <div style={{ marginTop: 8, textAlign: 'right' }}>
                                <Button type="primary" size="small" onClick={() => navigate('/volunteer/activity')}>
                                    查看全部
                                </Button>
                            </div>
                        </div>
                    ) : (
                        <p style={{ color: '#999' }}>暂无排班</p>
                    )}
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
                        <h3 style={{ margin: 0 }}>投喂任务</h3>
                        <Radio
                            options={[
                                { label: '全部', value: 'all' },
                                { label: '按ID', value: 'id' },
                                { label: '按点位', value: 'point' },
                                { label: '按状态', value: 'status' },
                            ]}
                            value={feedingFilter}
                            onChange={(v) => { setFeedingFilter(String(v)); setFeedingInput(''); setStatusFilter('') }}
                        />
                    </div>
                    <div style={{ maxHeight: 132, overflowY: 'auto' }}>
                        <Table columns={feedingColumns} dataSource={feedingTasks} />
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginTop: 8 }}>
                        <div style={{ display: 'flex', gap: 8 }}>
                            {(feedingFilter === 'id' || feedingFilter === 'point') && (
                                <Input
                                    placeholder={feedingFilter === 'id' ? '请输入任务ID' : '请输入点位ID'}
                                    value={feedingInput}
                                    onChange={(e: any) => setFeedingInput(e.target.value)}
                                    style={{ width: 160 }}
                                />
                            )}
                            {feedingFilter === 'status' && (
                                <Radio
                                    options={[
                                        { label: '计划', value: 'PLANNED' },
                                        { label: '已分配', value: 'ASSIGNED' },
                                        { label: '执行中', value: 'IN_PROGRESS' },
                                        { label: '已完成', value: 'COMPLETED' },
                                        { label: '逾期', value: 'MISSED' },
                                    ]}
                                    value={statusFilter}
                                    onChange={(v) => setStatusFilter(String(v))}
                                />
                            )}
                        </div>
                        <Button type="primary" size="small" onClick={queryFeedingTasks}>查询</Button>
                    </div>
                </div>
            </div>
        </>
    )
}