import {useEffect,useState} from 'react'
import { useNavigate } from 'react-router-dom'
import {Table,type TableColumn,Button,Icon,Notification,Drawer,Form,FormItem,useForm,Input,Radio} from 'animal-island-ui'
import {PageHeader} from '../../../shared/components/PageHeader'
import {StatusTag} from '../../../shared/components/StatusTag'
import {VolunteerService,visitTypeLabels} from '../../../services/volunteer.service'
import { DatePicker } from 'antd'

const loadData=async ()=>{
  return await VolunteerService.getVisitList()
}

const loadApprovedApps=async ()=>{
  return await VolunteerService.getApprovedApplications()
}

export function VisitPage({ embedded = false }: { embedded?: boolean }) {
  const navigate = useNavigate()
  const [data,setData]=useState<any[]>([])
  const [approvedApps,setApprovedApps]=useState<any[]>([])
  const [appQuery,setAppQuery]=useState('')
  const [visitQuery,setVisitQuery]=useState('')
  const [drawerOpen,setDrawerOpen]=useState(false)
  const [currentAppId,setCurrentAppId]=useState('')
  const [currentCatName,setCurrentCatName]=useState('')
  const [visitForm]=useForm()

  useEffect(()=>{
    loadData().then(setData).catch(()=>{
      Notification.error('数据加载失败')
    })
  },[])

  useEffect(()=>{
    loadApprovedApps().then(setApprovedApps).catch(()=>{
      Notification.error('已通过申请加载失败')
    })
  },[])

  const openDrawer=(appId:string,catName:string)=>{
    setCurrentAppId(appId)
    setCurrentCatName(catName)
    setDrawerOpen(true)
  }

  const closeDrawer=()=>{
    setDrawerOpen(false)
    visitForm.resetFields()
  }

  const handleCreateVisit=async ()=>{
    const values=visitForm.getFieldsValue()
    if(!values.visitType){
      Notification.error('请选择回访类型')
      return
    }
    if(values.passFlag===undefined||values.passFlag===null){
      Notification.error('请选择是否通过')
      return
    }
    const payload:Record<string,unknown>={
      visitType:values.visitType,
      passFlag:Number(values.passFlag),
    }
    if(values.visitTime){
      payload.visitTime=(values.visitTime as any).toISOString?.()??values.visitTime
    }
    if(values.conclusion){
      payload.conclusion=values.conclusion
    }

    try{
      await VolunteerService.createVisit(currentAppId,payload)
      Notification.success('回访记录新增成功')
      closeDrawer()
      loadData().then(setData).catch(()=>{})
    }catch{
      Notification.error('新增回访记录失败')
    }
  }

  const appColumns:TableColumn<any>[]=[
  {
    title:'申请编号',
    dataIndex:'applicationId'
  },{
    title:'猫咪名称',
    dataIndex:'catName',
    render:(text:any)=><span>{text||'-'}</span>
  },{
    title:'申请人',
    dataIndex:'applicantName',
    render:(text:any)=><span>{text||'-'}</span>
  },{
    title:'申请时间',
    dataIndex:'applyTime',
    render:(text:any)=><span>{text?text.format('YYYY-MM-DD HH:mm'):'-'}</span>
  },{
    title:'当前状态',
    dataIndex:'currentStatus',
    render:(text:any)=>{
      const map:Record<string,{value:string;label:string}>={
        APPROVED:{value:'COMPLETED',label:'已通过'},
      }
      const item=map[text]
      return item?<StatusTag value={item.value} label={item.label}/>:<span>{text||'-'}</span>
    }
  },{
    title:'操作',
    dataIndex:'applicationId',
    render:(_text:any,_record:any)=>{
      return <Button type="primary" size="small" onClick={()=>openDrawer(_record.applicationId,_record.catName)}>新建回访记录</Button>
    }
  }]

  const visitColumns:TableColumn<any>[]=[
  {
    title:'回访编号',
    dataIndex:'visitId'
  },{
    title:'申请编号',
    dataIndex:'applicationId'
  },{
    title:'猫咪id',
    dataIndex:'catId'
  },{
    title:'回访类型',
    dataIndex:'visitType',
    render: (text: any) => {
      const map: Record<string, {value: string; label: string}> = {
        INITIAL: {value: 'ACTIVE', label: '初次回访'},
        FOLLOW_UP: {value: 'PROCESSING', label: '跟进回访'},
        FINAL: {value: 'COMPLETED', label: '最终回访'},
      }
      const item = map[text]
      return item ? <StatusTag value={item.value} label={item.label} /> : <span>{text || '-'}</span>
    }
  },{
    title:'回访时间',
    dataIndex:'visitTime',
    render: (text: any) => <span>{text ? text.format('YYYY-MM-DD HH:mm') : '-'}</span>
  },{
    title:'回访人id',
    dataIndex:'visitorUserId'
  },{
    title:'回访结论',
    dataIndex:'conclusion',
    render: (text: any) => <span>{text || '-'}</span>
  },{
    title:'是否通过',
    dataIndex:'passFlag',
    render: (text: any) => {
      if (text === 1) return <StatusTag value="VERIFIED" label="通过" />
      if (text === 0) return <StatusTag value="REJECTED" label="未通过" />
      return <span>-</span>
    }
  },{
    title:'当前状态',
    dataIndex:'currentStatus'
  }]
  const filteredApprovedApps=approvedApps.filter((item:any)=>`${item.applicationId??''} ${item.catName??''} ${item.applicantName??''}`.toLowerCase().includes(appQuery.trim().toLowerCase()))
  const filteredVisits=data.filter((item:any)=>`${item.visitId??''} ${item.applicationId??''} ${item.catId??''} ${item.visitType??''} ${item.conclusion??''}`.toLowerCase().includes(visitQuery.trim().toLowerCase()))
  return (
    <>
      <div className='visit-page'>
        {!embedded && <div className='visit-page-header'>
          <PageHeader kicker='Visit Records' title="回访记录" description='查看已完成领养的回访记录，并对已通过申请新建回访' icon='icon-design'
            actions={<Button type="text" size="small" onClick={() => navigate('/volunteer')}><Icon name="icon-miles" size={15} />返回</Button>} />
        </div>}
        <div className='visit-page-section'>
          <div style={{display:'flex',alignItems:'center',justifyContent:'space-between',gap:12,marginBottom:12}}><h3 style={{margin:0}}>已通过领养申请</h3><Input value={appQuery} onChange={(e:any)=>setAppQuery(e.target.value)} placeholder='搜索申请或猫咪' style={{maxWidth:240}} /></div>
          <div className='visit-page-table'>
            <Table columns={appColumns} dataSource={filteredApprovedApps} />
          </div>
        </div>
        <div className='visit-page-section' style={{marginTop:24}}>
          <div style={{display:'flex',alignItems:'center',justifyContent:'space-between',gap:12,marginBottom:12}}><h3 style={{margin:0}}>回访记录列表</h3><Input value={visitQuery} onChange={(e:any)=>setVisitQuery(e.target.value)} placeholder='搜索回访记录' style={{maxWidth:240}} /></div>
          <div className='visit-page-table'>
            <Table columns={visitColumns} dataSource={filteredVisits} />
          </div>
        </div>
        <Drawer open={drawerOpen} onClose={closeDrawer} title={`新建回访记录${currentCatName?' — '+currentCatName:''}`}>
          <Form form={visitForm} layout='vertical'>
            <FormItem label='回访类型' name='visitType' required>
              <Radio options={[
                {label:visitTypeLabels.INITIAL,value:'INITIAL'},
                {label:visitTypeLabels.FOLLOW_UP,value:'FOLLOW_UP'},
                {label:visitTypeLabels.FINAL,value:'FINAL'},
              ]}/>
            </FormItem>
            <FormItem label='回访时间' name='visitTime'>
              <DatePicker showTime />
            </FormItem>
            <FormItem label='回访结论' name='conclusion'>
              <Input placeholder='请输入回访结论（选填）' />
            </FormItem>
            <FormItem label='是否通过' name='passFlag' required>
              <Radio options={[
                {label:'通过',value:1},
                {label:'未通过',value:0},
              ]}/>
            </FormItem>
            <FormItem>
              <Button type='primary' style={{marginRight:8}} onClick={handleCreateVisit}>提交</Button>
              <Button type='default' onClick={closeDrawer}>取消</Button>
            </FormItem>
          </Form>
        </Drawer>
      </div>
    </>
  );
}
