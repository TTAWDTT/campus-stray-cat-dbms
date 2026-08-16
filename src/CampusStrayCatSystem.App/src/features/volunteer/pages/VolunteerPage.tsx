import { Card, Icon, Button, Table, type TableColumn, Drawer, Modal, Form, FormItem, useForm, Input, Notification ,Radio,Tabs} from 'animal-island-ui'
import type { KeyboardEvent } from 'react'
import { useEffect, useMemo, useState } from 'react'
import { VolunteerService, shiftStatusLabels } from '../../../services/volunteer.service'
import { useAuthStore } from '../../../stores/auth.store'
import { StatusTag } from '../../../shared/components/StatusTag'
import { PageHeader } from '../../../shared/components/PageHeader'
import { DatePicker } from 'antd'
import { AdoptionCheckPage } from './AdoptionCheckPage'
import { VisitPage } from './VisitPage'

export function VolunteerPage() {
    const [pendingCount, setPendingCount] = useState(0)
    const [myShifts, setMyShifts] = useState<any[]>([])
    const [volunteerId, setVolunteerId] = useState('')
    const [recruitOpen, setRecruitOpen] = useState(false)
    const [creditLogOpen, setCreditLogOpen] = useState(false)
    const [recruitForm] = useForm()
    const [creditForm] = useForm()
    const [feedingTasks, setFeedingTasks] = useState<any[]>([])
    const [feedingRecords, setFeedingRecords] = useState<any[]>([])
    const [receivedHandovers, setReceivedHandovers] = useState<any[]>([])
    const [sentHandovers, setSentHandovers] = useState<any[]>([])
    const [allHandovers, setAllHandovers] = useState<any[]>([])
    const [handoverFilter, setHandoverFilter] = useState('all')
    const [handoverInput, setHandoverInput] = useState('')
    const [handoverStatusFilter, setHandoverStatusFilter] = useState('')
    const [handoverRelatedType, setHandoverRelatedType] = useState('SHIFT')
    const [handoverRelatedId, setHandoverRelatedId] = useState('')
    const [detailModal, setDetailModal] = useState<'shift'|'feeding'|'handover'|'adoption'|'visit'|null>(null)
    const [shiftQuery, setShiftQuery] = useState('')
    const [feedingQuery, setFeedingQuery] = useState('')
    const [feedingRecordQuery, setFeedingRecordQuery] = useState('')
    const isAdmin = (useAuthStore.getState().user?.roleName?.toUpperCase() === 'ADMIN')
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
        VolunteerService.getVolunteerProfile().then((profile) => {
            if (profile?.volunteerId) {
                setVolunteerId(profile.volunteerId)
            }
        }).catch(() => {})
    }, [])

    useEffect(() => {
        VolunteerService.getAllFeedingTasks().then(setFeedingTasks).catch(() => setFeedingTasks([]))
        VolunteerService.getFeedingRecords().then(setFeedingRecords).catch(() => setFeedingRecords([]))
    }, [])

    useEffect(() => {
        if (!volunteerId) return
        VolunteerService.getHandoverRecordsByTo(volunteerId).then(setReceivedHandovers).catch(() => setReceivedHandovers([]))
        VolunteerService.getHandoverRecordsByFrom(volunteerId).then(setSentHandovers).catch(() => setSentHandovers([]))
    }, [volunteerId])

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
        if (!volunteerId) return
        VolunteerService.getHandoverRecordsByTo(volunteerId).then(setReceivedHandovers).catch(() => {})
        VolunteerService.getHandoverRecordsByFrom(volunteerId).then(setSentHandovers).catch(() => {})
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
    const feedingRecordColumns: TableColumn<any>[] = [
        { title: '记录ID', dataIndex: 'CheckInID' },
        { title: '任务ID', dataIndex: 'ShiftID' },
        { title: '签到时间', dataIndex: 'CheckInTime', render: (t:any) => t ? t.format('MM-DD HH:mm') : '-' },
        { title: '距离（米）', dataIndex: 'DistanceMeters', render: (t:any) => t ?? '-' },
        { title: '状态', dataIndex: 'CheckInStatus', render: (t:any) => t === 'CHECKED_IN' ? '已签到' : t === 'LATE' ? '迟到' : t || '-' },
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
                    return handoverStatus==='PENDING'?(
                        <div style={{display:'flex',gap:4}}>
                            <Button type='default' size="small" onClick={() => handleConfirmHandover(handoverID)}>确认</Button>
                            <Button type='default' size="small" onClick={() => handleRejectHandover(handoverID)}>拒绝</Button>
                            <Button type='default' size="small" onClick={() => handleCancelHandover(handoverID)}>撤销</Button>
                        </div>
                    ):null
                }
            })
        }
        return cols
    },[allHandovers,receivedHandovers,sentHandovers])

    const filteredShifts = myShifts.filter((item:any) => {
        const text = `${item.shiftId ?? ''} ${item.userName ?? ''} ${item.shiftStatus ?? ''}`.toLowerCase()
        return text.includes(shiftQuery.trim().toLowerCase())
    })
    const filteredFeedingTasks = feedingTasks.filter((item:any) => {
        const text = `${item.shiftID ?? ''} ${item.volunteerID ?? ''} ${item.pointID ?? ''} ${item.shiftStatus ?? ''}`.toLowerCase()
        return text.includes(feedingQuery.trim().toLowerCase())
    })
    const filteredFeedingRecords = feedingRecords.filter((item:any) => {
        const text = `${item.CheckInID ?? ''} ${item.ShiftID ?? ''} ${item.CheckInStatus ?? ''}`.toLowerCase()
        return text.includes(feedingRecordQuery.trim().toLowerCase())
    })
    const detailModalTitle = detailModal === 'shift' ? '排班任务' : detailModal === 'feeding' ? '投喂任务和记录' : detailModal === 'handover' ? '交接事宜' : detailModal === 'adoption' ? '领养审核' : '回访汇总'

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

            <div className="finance-hero-grid volunteer-work-grid" style={{ marginTop:20 }}>
                <div className="finance-hero-card-hit" role="button" tabIndex={0} aria-label="查看排班任务"
                    onClick={() => setDetailModal('shift')} onKeyDown={activateCard(() => setDetailModal('shift'))}>
                    <Card color="app-teal" className="finance-hero-card">
                        <div className="finance-hero-card-inner">
                            <span className="finance-hero-icon"><Icon name="icon-miles" size={28} /></span>
                            <h2>排班任务</h2>
                            <p>{myShifts.length ? `当前有 ${myShifts.length} 条排班，点击查看完整安排` : '暂无排班任务，点击查看完整安排'}</p>
                        </div>
                    </Card>
                </div>
                <div className="finance-hero-card-hit" role="button" tabIndex={0} aria-label="查看投喂任务和记录"
                    onClick={() => setDetailModal('feeding')} onKeyDown={activateCard(() => setDetailModal('feeding'))}>
                    <Card color="app-blue" className="finance-hero-card">
                        <div className="finance-hero-card-inner">
                            <span className="finance-hero-icon"><Icon name="icon-shopping" size={28} /></span>
                            <h2>投喂任务和记录</h2>
                            <p>{feedingTasks.length ? `已有 ${feedingTasks.length} 条任务，点击查看任务与签到记录` : '暂无投喂任务，点击查看完整记录'}</p>
                        </div>
                    </Card>
                </div>
                <div className="finance-hero-card-hit" role="button" tabIndex={0} aria-label="查看交接事宜"
                    onClick={() => setDetailModal('handover')} onKeyDown={activateCard(() => setDetailModal('handover'))}>
                    <Card color="app-orange" className="finance-hero-card">
                        <div className="finance-hero-card-inner">
                            <span className="finance-hero-icon"><Icon name="icon-chat" size={28} /></span>
                            <h2>交接事宜</h2>
                            <p>{allHandovers.length ? `最近有 ${allHandovers.length} 条交接，点击查看状态` : '暂无交接记录，点击查看完整内容'}</p>
                        </div>
                    </Card>
                </div>
            </div>
            <Modal title={detailModalTitle} width={detailModal === 'adoption' || detailModal === 'visit' ? 1000 : 760} open={detailModal !== null} onClose={() => setDetailModal(null)}>
                {detailModal === 'adoption' && <AdoptionCheckPage embedded />}
                {detailModal === 'visit' && <VisitPage embedded />}
                {detailModal === 'shift' && <>
                    <div style={{display:'flex',justifyContent:'flex-end',marginBottom:12}}><Input value={shiftQuery} onChange={(e:any)=>setShiftQuery(e.target.value)} placeholder="按编号、志愿者或状态筛选" /></div>
                    <div className="volunteer-modal-table" style={{maxHeight:'min(56vh,520px)',overflowY:'auto',overflowX:'auto',scrollbarWidth:'thin',scrollbarColor:'rgba(121,79,39,.28) transparent'}}>
                        <Table columns={shiftColumns} dataSource={filteredShifts} />
                    </div>
                </>}
                {detailModal === 'feeding' && <>
                    <div style={{display:'flex',justifyContent:'flex-end',marginBottom:12}}><Input value={feedingQuery} onChange={(e:any)=>setFeedingQuery(e.target.value)} placeholder="按任务、志愿者、点位或状态筛选" /></div>
                    <div className="volunteer-modal-table" style={{maxHeight:'min(56vh,520px)',overflowY:'auto',overflowX:'auto',scrollbarWidth:'thin',scrollbarColor:'rgba(121,79,39,.28) transparent'}}>
                        <Table columns={feedingColumns} dataSource={filteredFeedingTasks} />
                    </div>
                    <div className="volunteer-records-heading"><h4>投喂记录</h4><Input value={feedingRecordQuery} onChange={(e:any)=>setFeedingRecordQuery(e.target.value)} placeholder="按记录ID、任务ID或状态筛选" /></div>
                    <div className="volunteer-modal-table" style={{maxHeight:220,overflowY:'auto',overflowX:'auto',scrollbarWidth:'thin',scrollbarColor:'rgba(121,79,39,.28) transparent'}}>
                        <Table columns={feedingRecordColumns} dataSource={filteredFeedingRecords} />
                    </div>
                </>}
                {detailModal === 'handover' && <div className="volunteer-modal-table" style={{maxHeight:'min(56vh,520px)',overflowY:'auto',overflowX:'auto',scrollbarWidth:'thin',scrollbarColor:'rgba(121,79,39,.28) transparent'}}>
                    <Tabs items={isAdmin ? [{key:'tab3',label:'全部交接',children:<>
                        <div style={{display:'grid',gap:10,marginBottom:12}}>
                            <Radio options={[{label:'全部',value:'all'},{label:'按ID',value:'id'},{label:'按状态',value:'status'},{label:'按关联',value:'related'}]} value={handoverFilter} onChange={(v)=>{setHandoverFilter(String(v));setHandoverInput('');setHandoverStatusFilter('');setHandoverRelatedId('')}} />
                            {handoverFilter==='id' && <Input placeholder='请输入交接ID' value={handoverInput} onChange={(e:any)=>setHandoverInput(e.target.value)} />}
                            {handoverFilter==='status' && <Radio options={[{label:'待确认',value:'PENDING'},{label:'已确认',value:'CONFIRMED'},{label:'已拒绝',value:'REJECTED'},{label:'已撤销',value:'CANCELLED'}]} value={handoverStatusFilter} onChange={(v)=>setHandoverStatusFilter(String(v))} />}
                            {handoverFilter==='related' && <Input placeholder='关联任务ID' value={handoverRelatedId} onChange={(e:any)=>setHandoverRelatedId(e.target.value)} />}
                            <Button type="primary" size="small" onClick={queryAllHandovers}>查询</Button>
                        </div>
                        <Table columns={AllHandoverColumns} dataSource={allHandovers} />
                    </>} ] : [{key:'tab1',label:'我收到的',children:<Table columns={ReceivedHandoverColumns} dataSource={receivedHandovers} />},{key:'tab2',label:'我发起的',children:<Table columns={SentHandoverColumns} dataSource={sentHandovers} />}]} defaultActiveKey={isAdmin?'tab3':'tab1'} />
                </div>}
            </Modal>
            <div className={isAdmin ? 'finance-hero-grid volunteer-tools-grid admin' : 'finance-hero-grid volunteer-tools-grid'} style={{ marginTop: 20 }}>
                <div className="finance-hero-card-hit" role="button" tabIndex={0}
                    aria-label="领养审核"
                    onClick={() => setDetailModal('adoption')}
                    onKeyDown={activateCard(() => setDetailModal('adoption'))}>
                    <Card color="app-red" className="finance-hero-card">
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
                    onClick={() => setDetailModal('visit')}
                    onKeyDown={activateCard(() => setDetailModal('visit'))}>
                    <Card color="purple" className="finance-hero-card">
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
        </>
    )
}
