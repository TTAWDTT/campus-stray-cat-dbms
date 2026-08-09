import {useEffect,useState,useRef} from 'react'
import { useNavigate } from 'react-router-dom'
import {Table,type TableColumn,Button,Icon,Notification,Tabs,Drawer,Form,FormItem,useForm,Input} from 'animal-island-ui'
import {PageHeader} from '../../../shared/components/PageHeader'
import {StatusTag} from '../../../shared/components/StatusTag'
import {VolunteerService} from '../../../services/volunteer.service'
import {useAuthStore} from '../../../stores/auth.store'
import { DatePicker } from 'antd'

const loadData=async ()=>{
  return await VolunteerService.getActivity()
}

export function ActivityPage() {
  const navigate = useNavigate()
  const [data,setData]=useState<any[]>([])
  const [activeKey,setActiveKey]=useState('mine')

  const userId = useAuthStore.getState().user?.userId
  const shiftPriority: Record<string, number> = { IN_PROGRESS: 0, PLANNED: 1, ASSIGNED: 2, COMPLETED: 3, MISSED: 4 }
  const myShifts = (userId ? data.filter((item: any) => item.volunteerId === userId) : data)
    .sort((a: any, b: any) => (shiftPriority[a.shiftStatus] ?? 99) - (shiftPriority[b.shiftStatus] ?? 99))
  const [DrawerOpen, setDrawerOpen] = useState(false)
  const [currentShiftId, setCurrentShiftId] = useState('')
  const [form]=useForm()
  const [previewUrl, setPreviewUrl] = useState<string>('')
  const fileRef = useRef<HTMLInputElement>(null)

  const openDrawer = (shiftId: string) => {
    setCurrentShiftId(shiftId)
    setDrawerOpen(true)
  }

  const closeDrawer = () => {
    setDrawerOpen(false)
    form.resetFields()
    setPreviewUrl('')
  }

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = () => {
      const dataUrl = reader.result as string
      setPreviewUrl(dataUrl)
      form.setFieldsValue({ photoUrl: dataUrl })
    }
    reader.readAsDataURL(file)
  }

  const handleSubmit = async () => {
    const values = form.getFieldsValue()
    const payload: Record<string, unknown> = {
      checkInStatus: 'CHECKED_IN',
    }
    if (values.checkInTime) {
      payload.checkInTime = (values.checkInTime as any).toISOString?.() ?? values.checkInTime
    }
    if (values.longitude) payload.longitude = Number(values.longitude)
    if (values.latitude) payload.latitude = Number(values.latitude)
    if (values.distanceMeters) payload.distanceMeters = Number(values.distanceMeters)
    if (values.photoUrl) payload.photoUrl = values.photoUrl

    try {
      await VolunteerService.checkInShift(currentShiftId, payload)
      Notification.success('签到成功')
      closeDrawer()
      loadData().then(setData).catch(() => {})
    } catch {
      Notification.error('签到失败')
    }
  }

  useEffect(()=>{
    loadData().then(setData).catch(()=>{
      Notification.error('数据加载失败')
    })
  },[])

  const baseColumns:TableColumn<any>[]=[
  {
    title:'志愿者',
    dataIndex:'userName'
  },{
    title:'信用等级',
    dataIndex:'creditLevel'
  },{
    title:'服务积分',
    dataIndex:'serviceScore'
  },{
    title:'在岗状态',
    dataIndex:'activeStatus',
    render: (text: any) => {
      const map: Record<string, {value: string; label: string}> = {
        ACTIVE: {value: 'ACTIVE', label: '在岗'},
        INACTIVE: {value: 'DISABLED', label: '离岗'},
      }
      const item = map[text]
      return item ? <StatusTag value={item.value} label={item.label} /> : <span>{text || '-'}</span>
    }
  },{
    title:'排班编号',
    dataIndex:'shiftId'
  },{
    title:'排班状态',
    dataIndex:'shiftStatus',
    render: (text: any) => {
      const map: Record<string, {value: string; label: string}> = {
        PLANNED: {value: 'PENDING', label: '已排班'},
        ASSIGNED: {value: 'PROCESSING', label: '已分配'},
        IN_PROGRESS: {value: 'ACTIVE', label: '执行中'},
        COMPLETED: {value: 'COMPLETED', label: '已完成'},
        MISSED: {value: 'REJECTED', label: '逾期'},
      }
      const item = map[text]
      return item ? <StatusTag value={item.value} label={item.label} /> : <span>{text || '-'}</span>
    }
  },{
    title:'开始时间',
    dataIndex:'planStartTime',
    render: (text: any) => <span>{text ? text.format('YYYY-MM-DD HH:mm') : '-'}</span>
  },{
    title:'结束时间',
    dataIndex:'planEndTime',
    render: (text: any) => <span>{text ? text.format('YYYY-MM-DD HH:mm') : '-'}</span>
  }
  ]

  const activityColumns: TableColumn<any>[] = [
    ...baseColumns,
    {
      title: '操作',
      dataIndex: 'shiftId',
      render: (_text: any, _record: any) => {
        const hide = _record.shiftStatus === 'COMPLETED' || _record.shiftStatus === 'MISSED'
        if (hide) return null
        return <Button type="primary" size="small" onClick={()=>openDrawer(_record.shiftId)}>签到</Button>
      },
    },
  ]
  return (
    <>
      <div className='activity-page'>
        <div className='activity-page-header'>
          <PageHeader kicker='Volunteer Activity' title="志愿者排班" description='查看所有志愿者的排班与活动状态' icon='icon-design'
            actions={<Button type="text" size="small" onClick={() => navigate('/volunteer')}><Icon name="icon-miles" size={15} />返回</Button>} />
        </div>
        <div className='activity-page-table'>
          <Tabs
            activeKey={activeKey}
            onChange={setActiveKey}
            items={[
              {
                key: 'mine',
                label: '我的排班',
                children: <Table columns={activityColumns} dataSource={myShifts} />,
              },
              {
                key: 'all',
                label: '全部排班',
                children: <Table columns={baseColumns} dataSource={data} />,
              },
            ]}
          />
        </div>
        <div className='activity-page-drawer'>
            <Drawer open={DrawerOpen} onClose={closeDrawer} title="签到(非必填)" >
                <Form form={form}
                    layout='vertical'>
                        <FormItem label='签到时间' name='checkInTime'>
                            <DatePicker></DatePicker>
                        </FormItem>
                        <FormItem label='经度' name='longitude'>
                            <Input placeholder='请输入经度' />
                        </FormItem>
                        <FormItem label='纬度' name='latitude'>
                            <Input placeholder='请输入纬度' />
                        </FormItem>
                        <FormItem label='图片' name='photoUrl'>
                            <input type="file" accept="image/*" ref={fileRef} onChange={handleFileChange} />
                            {previewUrl && <img src={previewUrl} alt="预览" style={{ marginTop: 8, maxWidth: '100%', maxHeight: 200, borderRadius: 4 }} />}
                        </FormItem>
                        <FormItem label='距离' name='distanceMeters'>
                            <Input placeholder='距离' />
                        </FormItem>
                        <FormItem>
                            <Button type='primary' style={{marginRight: 8}} onClick={handleSubmit}>提交</Button>
                            <Button type='default' onClick={closeDrawer}>取消</Button>
                        </FormItem>
                </Form>
            </Drawer>
        </div>
      </div>
    </>
  );
}