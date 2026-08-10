import { Card, Icon, Button, Table, type TableColumn, Drawer, Form, FormItem, useForm, Input, Notification ,Radio,Tabs} from 'animal-island-ui'
import { useNavigate } from 'react-router-dom'
import type { KeyboardEvent } from 'react'
import { useEffect, useMemo, useState } from 'react'
import { VolunteerService, shiftStatusLabels } from '../../../services/volunteer.service'
import { useAuthStore } from '../../../stores/auth.store'
import { StatusTag } from '../../../shared/components/StatusTag'
import { PageHeader } from '../../../shared/components/PageHeader'
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
    const [receivedHandovers, setReceivedHandovers] = useState<any[]>([])
    const [sentHandovers, setSentHandovers] = useState<any[]>([])
    const [allHandovers, setAllHandovers] = useState<any[]>([])
    const [handoverFilter, setHandoverFilter] = useState('all')
    const [handoverInput, setHandoverInput] = useState('')
    const [handoverStatusFilter, setHandoverStatusFilter] = useState('')
    const [handoverRelatedType, setHandoverRelatedType] = useState('SHIFT')
    const [handoverRelatedId, setHandoverRelatedId] = useState('')
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

    useEffect(() => {
        const volId = myShifts[0]?.volunteerId
        if (!volId) return
        VolunteerService.getHandoverRecordsByTo(volId).then(setReceivedHandovers).catch(() => setReceivedHandovers([]))
        VolunteerService.getHandoverRecordsByFrom(volId).then(setSentHandovers).catch(() => setSentHandovers([]))
    }, [myShifts])

    useEffect(() => {
        VolunteerService.getHandoverRecords().then(setAllHandovers).catch(() => setAllHandovers([]))
    }, [])

    const queryAllHandovers = async () => {
        try {
            if (handoverFilter === 'all') {
                const data = await VolunteerService.getHandoverRecords()
                setAllHandovers(data)
            } else if (handoverFilter === 'id') {
                if (!handoverInput) { Notification.error('请输入交接ID'); return }
                const data = await VolunteerService.getHandoverRecordsById(handoverInput)
                setAllHandovers(data)
            } else if (handoverFilter === 'status') {
                if (!handoverStatusFilter) { Notification.error('请选择状态'); return }
                const data = await VolunteerService.getHandoverRecordsByStatus(handoverStatusFilter)
                setAllHandovers(data)
            } else if (handoverFilter === 'related') {
                if (!handoverRelatedType || !handoverRelatedId) { Notification.error('请填写关联类型和关联ID'); return }
                const data = await VolunteerService.getHandoverRecordsByRelated(handoverRelatedType, handoverRelatedId)
                setAllHandovers(data)
            }
        } catch {
            Notification.error('查询失败')
        }
    }

    const refreshHandovers = () => {
        const volId = myShifts[0]?.volunteerId
        if (!volId) return
        VolunteerService.getHandoverRecordsByTo(volId).then(setReceivedHandovers).catch(() => {})
        VolunteerService.getHandoverRecordsByFrom(volId).then(setSentHandovers).catch(() => {})
        VolunteerService.getHandoverRecords().then(setAllHandovers).catch(() => {})
    }

    const handleConfirmHandover = async (handoverID: string) => {
        try {
            await VolunteerService.confirmHandover(handoverID)
            Notification.success('交接已确认')
            refreshHandovers()
        } catch {
            Notification.error('确认失败')
        }
    }

    const handleRejectHandover = async (handoverID: string) => {
        try {
            await VolunteerService.rejectHandover(handoverID)
            Notification.success('已拒绝交接')
            refreshHandovers()
        } catch {
            Notification.error('操作失败')
        }
    }

    const handleCancelHandover = async (handoverID: string) => {
        try {
            await VolunteerService.cancelHandover(handoverID)
            Notification.success('交接已撤销')
            refreshHandovers()
        } catch {
            Notification.error('撤销失败')
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
    
    const ReceivedHandoverColumns:TableColumn<any>[]=useMemo(()=>{
        const hasPending=receivedHandovers.some((h:any)=>h.handoverStatus==='PENDING')
        const cols:TableColumn<any>[]=[
            { title:'交接编号',dataIndex:'handoverID',width:100},
            { title:'发起人ID',dataIndex:'fromVolunteerID',width:100},
            { title:'关联任务',dataIndex:'relatedID',width:100},
            { title:'发起时间',dataIndex:'applyTime',width:150,render:(t:any)=>t?t.format('MM-DD HH:mm'):'-'},
            {
                title:'状态',dataIndex:'handoverStatus',
                render:(t:any)=>{
                    const map:Record<string,{value:string;label:string}>={
                        PENDING:{value:'PROCESSING',label:'待确认'},
                        CONFIRMED:{value:'COMPLETED',label:'已确认'},
                        REJECTED:{value:'REJECTED',label:'已拒绝'},
                        CANCELLED:{value:'PENDING',label:'已撤销'},
                    }
                    const item=map[t]
                    return item?<StatusTag value={item.value} label={item.label}/>:<span>{t||'-'}</span>
                },
            },
        ]
        if(hasPending){
            cols.push({
                title:'操作',
                render:(_,value)=>{
                    const handoverID=value.handoverID
                    const handoverStatus=value.handoverStatus as string
                    return handoverStatus==='PENDING'?(
                        <div style={{display:'flex',gap:4}}>
                            <Button type='primary' size="small" onClick={() => handleConfirmHandover(handoverID)}>确认</Button>
                            <Button type='default' size="small" onClick={() => handleRejectHandover(handoverID)}>拒绝</Button>
                        </div>
                    ):null
                }
            })
        }
        return cols
    },[receivedHandovers])

    const SentHandoverColumns:TableColumn<any>[]=useMemo(()=>{
        const hasPending=sentHandovers.some((h:any)=>h.handoverStatus==='PENDING')
        const cols:TableColumn<any>[]=[
            {title:'交接编号',dataIndex:'handoverID',width:100},
            {title:'接收人ID',dataIndex:'toVolunteerID',width:100},
            {title:'关联任务',dataIndex:'relatedID',width:100},
            { title:'发起时间',dataIndex:'applyTime',width:150,render:(t:any)=>t?t.format('MM-DD HH:mm'):'-'},
            {
                title:'状态',dataIndex:'handoverStatus',
                render:(t:any)=>{
                    const map:Record<string,{value:string;label:string}>={
                        PENDING:{value:'PROCESSING',label:'待确认'},
                        CONFIRMED:{value:'COMPLETED',label:'已确认'},
                        REJECTED:{value:'REJECTED',label:'已拒绝'},
                        CANCELLED:{value:'PENDING',label:'已撤销'},
                    }
                    const item=map[t]
                    return item?<StatusTag value={item.value} label={item.label}/>:<span>{t||'-'}</span>
                },
            },
        ]
        if(hasPending){
            cols.push({
                title:'操作',
                render:(_,value)=>{
                    const handoverID=value.handoverID
                    const handoverStatus=value.handoverStatus as string
                    return handoverStatus==='PENDING'?(
                        <Button type='default' size="small" onClick={() => handleCancelHandover(handoverID)}>撤销</Button>
                    ): null
                }
            })
        }
        return cols
    },[sentHandovers])

    const AllHandoverColumns:TableColumn<any>[]=useMemo(()=>{
        const hasPending=allHandovers.some((h:any)=>h.handoverStatus==='PENDING')
        const cols:TableColumn<any>[]=[
            {title:'交接编号',dataIndex:'handoverID',width:100},
            {title:'发起人ID',dataIndex:'fromVolunteerID',width:100},
            {title:'接收人ID',dataIndex:'toVolunteerID',width:100},
            {title:'关联任务',dataIndex:'relatedID',width:100},
            {title:'发起时间',dataIndex:'applyTime',width:150,render:(t:any)=>t?t.format('MM-DD HH:mm'):'-'},
            {
                title:'状态',dataIndex:'handoverStatus',
                render:(t:any)=>{
                    const map:Record<string,{value:string;label:string}>={
                        PENDING:{value:'PROCESSING',label:'待确认'},
                        CONFIRMED:{value:'COMPLETED',label:'已确认'},
                        REJECTED:{value:'REJECTED',label:'已拒绝'},
                        CANCELLED:{value:'PENDING',label:'已撤销'},
                    }
                    const item=map[t]
                    return item?<StatusTag value={item.value} label={item.label}/>:<span>{t||'-'}</span>
                },
            },
        ]
        if(hasPending){
            cols.push({
                title:'操作',
                render:(_,value)=>{
                    const handoverID=value.handoverID
                    const handoverStatus=value.handoverStatus as string
                    const isReceived=receivedHandovers.some((h:any)=>h.handoverID===handoverID)
                    const isSent=sentHandovers.some((h:any)=>h.handoverID===handoverID)
                    return handoverStatus==='PENDING'?(
                        <div style={{display:'flex',gap:4}}>
                            {isReceived&&<Button type='primary' size="small" onClick={() => handleConfirmHandover(handoverID)}>确认</Button>}
                            {isReceived&&<Button type='default' size="small" onClick={() => handleRejectHandover(handoverID)}>拒绝</Button>}
                            {isSent&&<Button type='default' size="small" onClick={() => handleCancelHandover(handoverID)}>撤销</Button>}
                        </div>
                    ):null
                }
            })
        }
        return cols
    },[allHandovers,receivedHandovers,sentHandovers])

    return (
        <>
            <PageHeader kicker="Volunteer" title="志愿者中心" description="管理领养审核、回访记录、排班任务、投喂任务和交接事宜" icon="icon-design" />

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
                    <h3 style={{ margin: '0 0 12px 0' }}>排班任务</h3>
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
                    <h3 style={{ margin: '0 0 12px 0' }}>投喂任务和记录</h3>
                    <div style={{ maxHeight: 132, overflowY: 'auto' }}>
                        <Table columns={feedingColumns} dataSource={feedingTasks} />
                    </div>
                    <div style={{ marginTop: 8, textAlign: 'right' }}>
                        <Button type="primary" size="small" onClick={() => {
                          const volId = myShifts[0]?.volunteerId
                          navigate(`/volunteer/feeding-tasks${volId ? `?volunteerId=${encodeURIComponent(volId)}` : ''}`)
                        }}>查看全部</Button>
                    </div>
                </div>
            </div>
            <div className="finance-hero-grid" style={{ gridTemplateColumns: isAdmin ? 'repeat(4, 1fr)' : 'repeat(2, 1fr)', marginTop: 20 }}>
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
            <h3 style={{ marginTop: 20 }}>交接事宜</h3>
            <div style={{ padding: 16 }}>
                <Tabs items={[{
                    key:'tab1',label:'我收到的',children:<div style={{ maxHeight: 180, overflowY: 'auto' }}><Table columns={ReceivedHandoverColumns} dataSource={receivedHandovers} /></div>
                },{
                    key:'tab2',label:'我发起的',children:<div style={{ maxHeight: 180, overflowY: 'auto' }}><Table columns={SentHandoverColumns} dataSource={sentHandovers} /></div>
                },{
                    key:'tab3',label:'全部交接',children:<>
                        <div style={{ display:'flex',alignItems:'center',justifyContent:'space-between',marginBottom:12 }}>
                            <Radio
                                options={[
                                    {label:'全部',value:'all'},
                                    {label:'按ID',value:'id'},
                                    {label:'按状态',value:'status'},
                                    {label:'按关联',value:'related'},
                                ]}
                                value={handoverFilter}
                                onChange={(v)=>{setHandoverFilter(String(v));setHandoverInput('');setHandoverStatusFilter('');setHandoverRelatedId('')}}
                            />
                            <Button type="primary" size="small" onClick={queryAllHandovers}>查询</Button>
                        </div>
                        <div style={{ display:'flex',alignItems:'center',gap:8,marginBottom:12 }}>
                            {handoverFilter==='id'&&(
                                <Input placeholder='请输入交接ID' value={handoverInput} onChange={(e:any)=>setHandoverInput(e.target.value)} style={{width:160}} />
                            )}
                            {handoverFilter==='status'&&(
                                <Radio
                                    options={[
                                        {label:'待确认',value:'PENDING'},
                                        {label:'已确认',value:'CONFIRMED'},
                                        {label:'已拒绝',value:'REJECTED'},
                                        {label:'已撤销',value:'CANCELLED'},
                                    ]}
                                    value={handoverStatusFilter}
                                    onChange={(v)=>setHandoverStatusFilter(String(v))}
                                />
                            )}
                            {handoverFilter==='related'&&(
                                <>
                                    <Radio
                                        options={[{label:'投喂任务',value:'SHIFT'}]}
                                        value={handoverRelatedType}
                                        onChange={(v)=>setHandoverRelatedType(String(v))}
                                    />
                                    <div style={{width:130,flex:'none'}}>
                                        <Input placeholder='关联任务ID' value={handoverRelatedId} onChange={(e:any)=>setHandoverRelatedId(e.target.value)} />
                                    </div>
                                </>
                            )}
                        </div>
                        <div style={{ maxHeight: 180, overflowY: 'auto' }}>
                            <Table columns={AllHandoverColumns} dataSource={allHandovers} />
                        </div>
                    </>
                }]}
                 defaultActiveKey='tab1' />
            </div>
        </>
    )
}