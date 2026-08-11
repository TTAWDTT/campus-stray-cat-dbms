import {useEffect,useState,useRef} from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import {Table,type TableColumn,Button,Icon,Notification,Input,Radio,Drawer,Form,FormItem,useForm} from 'animal-island-ui'
import { DatePicker } from 'antd'
import {PageHeader} from '../../../shared/components/PageHeader'
import {StatusTag} from '../../../shared/components/StatusTag'
import {VolunteerService} from '../../../services/volunteer.service'
import {useAuthStore} from '../../../stores/auth.store'

export function FeedingTasksPage() {
  const navigate = useNavigate()
  const [searchParams]=useSearchParams()
  const myVolunteerId=searchParams.get('volunteerId')||''
  const isAdmin=(useAuthStore.getState().user?.roleName?.toUpperCase()==='ADMIN')
  const [feedingTasks,setFeedingTasks]=useState<any[]>([])
  const [feedingFilter,setFeedingFilter]=useState(isAdmin?'all':'mine')
  const [feedingInput,setFeedingInput]=useState('')
  const [statusFilter,setStatusFilter]=useState('')
  const [feedingRecords,setFeedingRecords]=useState<any[]>([])
  const [recordFilter,setRecordFilter]=useState('all')
  const [recordInput,setRecordInput]=useState('')
  const [checkInOpen,setCheckInOpen]=useState(false)
  const [currentShiftId,setCurrentShiftId]=useState('')
  const [checkInForm]=useForm()
  const [handoverForm]=useForm()
  const [previewUrl,setPreviewUrl]=useState<string>('')
  const fileRef=useRef<HTMLInputElement>(null)
  const [handoverOpen,setHandoverOpen]=useState(false)
  const [currentHandoverShiftId,setCurrentHandoverShiftId]=useState('')
  const [currentHandoverFromVolId,setCurrentHandoverFromVolId]=useState('')
  const [taskDrawerOpen,setTaskDrawerOpen]=useState(false)
  const [taskDrawerMode,setTaskDrawerMode]=useState<'create'|'update'>('create')
  const [editingTaskId,setEditingTaskId]=useState('')
  const [taskForm]=useForm()

  useEffect(()=>{
    VolunteerService.getAllFeedingTasks().then(setFeedingTasks).catch(()=>{
      Notification.error('投喂任务加载失败')
    })
    VolunteerService.getFeedingRecords().then(setFeedingRecords).catch(()=>{
      Notification.error('投喂记录加载失败')
    })
  },[])

  const queryFeedingTasks=async ()=>{
    try{
      if(feedingFilter==='all'){
        const data=await VolunteerService.getAllFeedingTasks()
        setFeedingTasks(data)
      }else if(feedingFilter==='id'){
        if(!feedingInput){Notification.error('请输入任务ID');return}
        const data=await VolunteerService.getFeedingTasksById(feedingInput)
        setFeedingTasks(data)
      }else if(feedingFilter==='point'){
        if(!feedingInput){Notification.error('请输入点位ID');return}
        const data=await VolunteerService.getFeedingTasksByPoint(feedingInput)
        setFeedingTasks(data)
      }else if(feedingFilter==='status'){
        if(!statusFilter){Notification.error('请选择状态');return}
        const data=await VolunteerService.getFeedingTasksByStatus(statusFilter)
        setFeedingTasks(data)
      }else if(feedingFilter==='mine'){
        if(!myVolunteerId){Notification.error('无法获取当前志愿者信息，请从志愿者主页进入');return}
        const data=await VolunteerService.getFeedingTasksByVolunteer(myVolunteerId)
        setFeedingTasks(data)
      }
    }catch{
      Notification.error('查询失败')
    }
  }

  const queryFeedingRecords=async ()=>{
    try{
      if(recordFilter==='all'){
        const data=await VolunteerService.getFeedingRecords()
        setFeedingRecords(data)
      }else if(recordFilter==='id'){
        if(!recordInput){Notification.error('请输入记录ID');return}
        const data=await VolunteerService.getFeedingRecordsById(recordInput)
        setFeedingRecords(data)
      }else if(recordFilter==='shift'){
        if(!recordInput){Notification.error('请输入任务ID');return}
        const data=await VolunteerService.getFeedingRecordsByShift(recordInput)
        setFeedingRecords(data)
      }else if(recordFilter==='mine'){
        if(!myVolunteerId){Notification.error('无法获取当前志愿者信息，请从志愿者主页进入');return}
        const data=await VolunteerService.getFeedingRecordsByVolunteer(myVolunteerId)
        setFeedingRecords(data)
      }
    }catch{
      Notification.error('查询失败')
    }
  }

  const openCheckIn=(shiftId:string)=>{
    setCurrentShiftId(shiftId)
    setCheckInOpen(true)
  }

  const closeCheckIn=()=>{
    setCheckInOpen(false)
    checkInForm.resetFields()
    setPreviewUrl('')
  }

  const handleFileChange=(e: React.ChangeEvent<HTMLInputElement>)=>{
    const file=e.target.files?.[0]
    if(!file) return
    const reader=new FileReader()
    reader.onload=()=>{
      const dataUrl=reader.result as string
      setPreviewUrl(dataUrl)
      checkInForm.setFieldsValue({photoUrl:dataUrl})
    }
    reader.readAsDataURL(file)
  }

  const handleCheckIn=async ()=>{
    const values:Record<string,any>=checkInForm.getFieldsValue()
    if(!values.checkInTime){Notification.error('请选择签到时间');return}
    if(!values.longitude){Notification.error('请输入经度');return}
    if(!values.latitude){Notification.error('请输入纬度');return}
    if(!values.photoUrl){Notification.error('请上传图片');return}
    if(!values.distanceMeters){Notification.error('请输入距离');return}
    if(!values.checkInStatus){Notification.error('请选择签到状态');return}
    const checkInTime:Date=values.checkInTime?(values.checkInTime as any).toDate():new Date()
    const checkInStatus:string=values.checkInStatus||'CHECKED_IN'
    try{
      await VolunteerService.postFeedingRecords(
        currentShiftId,
        checkInTime,
        Number(values.longitude)||0,
        Number(values.latitude)||0,
        String(values.photoUrl||''),
        Number(values.distanceMeters)||0,
        checkInStatus
      )
      Notification.success('签到成功')
      closeCheckIn()
      VolunteerService.getFeedingRecords().then(setFeedingRecords).catch(()=>{})
    }catch{
      Notification.error('签到失败')
    }
  }

  const openHandover=(shiftId:string,fromVolunteerId:string)=>{
    setCurrentHandoverShiftId(shiftId)
    setCurrentHandoverFromVolId(fromVolunteerId)
    setHandoverOpen(true)
  }

  const closeHandover=()=>{
    setHandoverOpen(false)
    handoverForm.resetFields()
  }

  const openTaskDrawer=(mode:'create'|'update',record?:any)=>{
    setTaskDrawerMode(mode)
    if(mode==='update'&&record){
      setEditingTaskId(record.shiftID)
      taskForm.setFieldsValue({
        volunteerID:record.volunteerID,
        pointID:record.pointID,
        backupVolunteerID:record.backupVolunteerID||'',
        planStartTime:record.planStartTime,
        planEndTime:record.planEndTime,
        shiftStatus:record.shiftStatus,
      })
    }else{
      setEditingTaskId('')
      taskForm.resetFields()
    }
    setTaskDrawerOpen(true)
  }

  const closeTaskDrawer=()=>{
    setTaskDrawerOpen(false)
    taskForm.resetFields()
  }

  const handleTaskSubmit=async ()=>{
    const values:Record<string,any>=taskForm.getFieldsValue()
    if(!values.volunteerID){Notification.error('请输入志愿者ID');return}
    if(!values.pointID){Notification.error('请输入点位ID');return}
    if(!values.planStartTime||!values.planEndTime){Notification.error('请选择时间');return}
    if(!values.shiftStatus){Notification.error('请选择状态');return}
    const startTime:Date=values.planStartTime?(values.planStartTime as any).toDate():new Date()
    const endTime:Date=values.planEndTime?(values.planEndTime as any).toDate():new Date()
    if(endTime<=startTime){Notification.error('结束时间不得早于开始时间');return}
    if(values.backupVolunteerID&&values.backupVolunteerID===values.volunteerID){Notification.error('备用志愿者不得与负责人相同');return}
    try{
      if(taskDrawerMode==='update'){
        await VolunteerService.putFeedingTasks(
          editingTaskId,
          String(values.volunteerID),
          String(values.pointID),
          String(values.backupVolunteerID||''),
          startTime,
          endTime,
          String(values.shiftStatus)
        )
        Notification.success('更新成功')
      }else{
        await VolunteerService.postFeedingTasks(
          String(values.volunteerID),
          String(values.pointID),
          String(values.backupVolunteerID||''),
          startTime,
          endTime,
          String(values.shiftStatus)
        )
        Notification.success('创建成功')
      }
      closeTaskDrawer()
      VolunteerService.getAllFeedingTasks().then(setFeedingTasks).catch(()=>{})
    }catch{
      Notification.error(taskDrawerMode==='update'?'更新失败':'创建失败')
    }
  }

  const handleHandover=async ()=>{
    const values:Record<string,any>=handoverForm.getFieldsValue()
    if(!values.toVolunteerID){Notification.error('请输入接收方志愿者ID');return}
    try{
      await VolunteerService.postHandover({
        fromVolunteerID:currentHandoverFromVolId,
        toVolunteerID:String(values.toVolunteerID),
        relatedType:'SHIFT',
        relatedID:currentHandoverShiftId,
        remark:String(values.remark||''),
      })
      Notification.success('交接发起成功')
      closeHandover()
    }catch{
      Notification.error('交接发起失败')
    }
  }

  const feedingColumns:TableColumn<any>[]=[
    {title:'任务ID',dataIndex:'shiftID'},
    {title:'志愿者',dataIndex:'volunteerID'},
    {title:'点位',dataIndex:'pointID'},
    {title:'开始时间',dataIndex:'planStartTime',render:(t:any)=>t?t.format('MM-DD HH:mm'):'-'},
    {title:'结束时间',dataIndex:'planEndTime',render:(t:any)=>t?t.format('MM-DD HH:mm'):'-'},
    {
      title:'状态',dataIndex:'shiftStatus',
      render:(t:any)=>{
        const map:Record<string,{value:string;label:string}>={
          PLANNED:{value:'PENDING',label:'已排班'},
          ASSIGNED:{value:'PROCESSING',label:'已分配'},
          IN_PROGRESS:{value:'ACTIVE',label:'执行中'},
          COMPLETED:{value:'COMPLETED',label:'已完成'},
          MISSED:{value:'REJECTED',label:'逾期'},
        }
        const item=map[t]
        return item?<StatusTag value={item.value} label={item.label}/>:<span>{t||'-'}</span>
      },
    },
    {
      title:'操作',dataIndex:'shiftID',
      render:(_text:any,_record:any)=>{
        const done=_record.shiftStatus==='COMPLETED'||_record.shiftStatus==='MISSED'
        if(done) return null
        return (
          <div style={{display:'flex',gap:4}}>
            {!isAdmin&&<Button type="primary" size="small" onClick={()=>openCheckIn(_record.shiftID)}>签到</Button>}
            <Button type="primary" size="small" onClick={()=>openHandover(_record.shiftID,_record.volunteerID)}>交接</Button>
            {isAdmin&&<Button type="primary" size="small" onClick={()=>openTaskDrawer('update',_record)}>更新</Button>}
          </div>
        )
      },
    }
  ]

  const recordColumns:TableColumn<any>[]=[
    {title:'记录ID',dataIndex:'CheckInID'},
    {title:'任务ID',dataIndex:'ShiftID'},
    {title:'签到时间',dataIndex:'CheckInTime',render:(t:any)=>t?t.format('MM-DD HH:mm'):'-'},
    {title:'经度',dataIndex:'Longitude'},
    {title:'纬度',dataIndex:'Latitude'},
    {title:'距离(m)',dataIndex:'DistanceMeters'},
    {
      title:'状态',dataIndex:'CheckInStatus',
      render:(t:any)=>{
        const map:Record<string,{value:string;label:string}>={
          CHECKED_IN:{value:'COMPLETED',label:'已签到'},
          LATE:{value:'REJECTED',label:'迟到'},
        }
        const item=map[t]
        return item?<StatusTag value={item.value} label={item.label}/>:<span>{t||'-'}</span>
      },
    }
  ]

  return (
    <>
      <div className='feeding-tasks-page'>
        <div className='feeding-tasks-page-header'>
          <PageHeader kicker='Feeding Tasks' title="投喂任务和记录" description='查看和管理所有投喂任务和记录，支持按ID、点位、状态筛选' icon='icon-design'
            actions={<Button type="text" size="small" onClick={()=>navigate('/volunteer')}><Icon name="icon-miles" size={15}/>返回</Button>}/>
        </div>
        <h3>投喂任务</h3>
        <div className='feeding-tasks-page-toolbar' style={{display:'flex',alignItems:'center',justifyContent:'space-between',marginBottom:12}}>
          <Radio
            options={[
              {label:'全部',value:'all'},
              {label:'按ID',value:'id'},
              {label:'按点位',value:'point'},
              {label:'按状态',value:'status'},
              ...(!isAdmin?[{label:'我的',value:'mine'}]:[]),
            ]}
            value={feedingFilter}
            onChange={(v)=>{setFeedingFilter(String(v));setFeedingInput('');setStatusFilter('')}}
          />
          <div style={{display:'flex',gap:8}}>
            <Button type="primary" size="small" onClick={queryFeedingTasks}>查询</Button>
            {isAdmin&&<Button type="primary" size="small" onClick={()=>openTaskDrawer('create')}>新增任务</Button>}
          </div>
        </div>
        <div style={{display:'flex',alignItems:'center',gap:8,marginBottom:12}}>
          {(feedingFilter==='id'||feedingFilter==='point')&&(
            <Input
              placeholder={feedingFilter==='id'?'请输入任务ID':'请输入点位ID'}
              value={feedingInput}
              onChange={(e:any)=>setFeedingInput(e.target.value)}
              style={{width:160}}
            />
          )}
          {feedingFilter==='status'&&(
            <Radio
              options={[
                {label:'计划',value:'PLANNED'},
                {label:'已分配',value:'ASSIGNED'},
                {label:'执行中',value:'IN_PROGRESS'},
                {label:'已完成',value:'COMPLETED'},
                {label:'逾期',value:'MISSED'},
              ]}
              value={statusFilter}
              onChange={(v)=>setStatusFilter(String(v))}
            />
          )}
        </div>
        <div className='feeding-tasks-page-table'>
          <Table columns={feedingColumns} dataSource={feedingTasks}/>
        </div>
        <h3 style={{marginTop:24}}>投喂记录</h3>
        <div className='feeding-tasks-page-toolbar' style={{display:'flex',alignItems:'center',justifyContent:'space-between',marginBottom:12}}>
          <Radio
            options={[
              {label:'全部',value:'all'},
              {label:'按ID',value:'id'},
              {label:'按任务',value:'shift'},
              ...(!isAdmin?[{label:'我的',value:'mine'}]:[]),
            ]}
            value={recordFilter}
            onChange={(v)=>{setRecordFilter(String(v));setRecordInput('')}}
          />
          <Button type="primary" size="small" onClick={queryFeedingRecords}>查询</Button>
        </div>
        <div style={{display:'flex',alignItems:'center',gap:8,marginBottom:12}}>
          {(recordFilter==='id'||recordFilter==='shift')&&(
            <Input
              placeholder={recordFilter==='id'?'请输入记录ID':'请输入任务ID'}
              value={recordInput}
              onChange={(e:any)=>setRecordInput(e.target.value)}
              style={{width:160}}
            />
          )}
        </div>
        <div className='feeding-tasks-page-table'>
          <Table columns={recordColumns} dataSource={feedingRecords}/>
        </div>
        <Drawer open={checkInOpen} onClose={closeCheckIn} title="投喂签到">
          <Form form={checkInForm} layout='vertical'>
            <FormItem label='签到时间' name='checkInTime' required rules={[{required:true,message:'请选择签到时间'}]}>
              <DatePicker showTime />
            </FormItem>
            <FormItem label='经度' name='longitude' required rules={[{required:true,message:'请输入经度'}]}>
              <Input placeholder='请输入经度' />
            </FormItem>
            <FormItem label='纬度' name='latitude' required rules={[{required:true,message:'请输入纬度'}]}>
              <Input placeholder='请输入纬度' />
            </FormItem>
            <FormItem label='图片' name='photoUrl' required rules={[{required:true,message:'请上传图片'}]}>
              <input type="file" accept="image/*" ref={fileRef} onChange={handleFileChange} />
              {previewUrl && <img src={previewUrl} alt="预览" style={{ marginTop: 8, maxWidth: '100%', maxHeight: 200, borderRadius: 4 }} />}
            </FormItem>
            <FormItem label='距离(m)' name='distanceMeters' required rules={[{required:true,message:'请输入距离'}]}>
              <Input placeholder='请输入距离' />
            </FormItem>
            <FormItem label='签到状态' name='checkInStatus' required rules={[{required:true,message:'请选择签到状态'}]}>
              <Radio options={[
                {label:'已签到',value:'CHECKED_IN'},
                {label:'迟到',value:'LATE'},
              ]}/>
            </FormItem>
            <FormItem>
              <Button type='primary' style={{marginRight:8}} onClick={handleCheckIn}>提交</Button>
              <Button type='primary' onClick={closeCheckIn}>取消</Button>
            </FormItem>
          </Form>
        </Drawer>
        <Drawer open={handoverOpen} onClose={closeHandover} title="发起交接">
          <Form form={handoverForm} layout='vertical'>
            <FormItem label='接收方志愿者ID' name='toVolunteerID' required>
              <Input placeholder='请输入接收方志愿者ID' />
            </FormItem>
            <FormItem label='备注' name='remark'>
              <Input placeholder='请输入备注（选填）' />
            </FormItem>
            <FormItem>
              <Button type='primary' style={{marginRight:8}} onClick={handleHandover}>提交</Button>
              <Button type='primary' onClick={closeHandover}>取消</Button>
            </FormItem>
          </Form>
        </Drawer>
        <Drawer open={taskDrawerOpen} onClose={closeTaskDrawer} title={taskDrawerMode==='update'?'更新投喂任务':'新增投喂任务'}>
          <Form form={taskForm} layout='vertical'>
            <FormItem label='志愿者ID' name='volunteerID' required>
              <Input placeholder='请输入志愿者ID' />
            </FormItem>
            <FormItem label='点位ID' name='pointID' required>
              <Input placeholder='请输入点位ID' />
            </FormItem>
            <FormItem label='备用志愿者ID' name='backupVolunteerID'>
              <Input placeholder='请输入备用志愿者ID（选填）' />
            </FormItem>
            <FormItem label='开始时间' name='planStartTime' required>
              <DatePicker showTime />
            </FormItem>
            <FormItem label='结束时间' name='planEndTime' required>
              <DatePicker showTime />
            </FormItem>
            <FormItem label='排班状态' name='shiftStatus' required>
              <Radio options={[
                {label:'已排班',value:'PLANNED'},
                {label:'已分配',value:'ASSIGNED'},
                {label:'执行中',value:'IN_PROGRESS'},
                {label:'已完成',value:'COMPLETED'},
                {label:'逾期',value:'MISSED'},
              ]}/>
            </FormItem>
            <FormItem>
              <Button type='primary' style={{marginRight:8}} onClick={handleTaskSubmit}>提交</Button>
              <Button type='primary' onClick={closeTaskDrawer}>取消</Button>
            </FormItem>
          </Form>
        </Drawer>
      </div>
    </>
  )
}