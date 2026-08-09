import {useEffect,useState} from 'react'
import { useNavigate } from 'react-router-dom'
import {Table,type TableColumn,Button,Icon,Notification} from 'animal-island-ui'
import {PageHeader} from '../../../shared/components/PageHeader'
import {StatusTag} from '../../../shared/components/StatusTag'
import {VolunteerService} from '../../../services/volunteer.service'

const loadData=async ()=>{
  return await VolunteerService.getVisitList()
}

export function VisitPage() {
  const navigate = useNavigate()
  const [data,setData]=useState<any[]>([])

  useEffect(()=>{
    loadData().then(setData).catch(()=>{
      Notification.error('数据加载失败')
    })
  },[])

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
  }
]
  return (
    <>
      <div className='visit-page'>
        <div className='visit-page-header'>
          <PageHeader kicker='Visit Records' title="回访记录" description='查看已完成领养的回访记录' icon='icon-design'
            actions={<Button type="text" size="small" onClick={() => navigate('/volunteer')}><Icon name="icon-miles" size={15} />返回</Button>} />
        </div>
        <div className='visit-page-table'>
          <Table columns={visitColumns} dataSource={data} />
        </div>
      </div>
    </>
  );
}