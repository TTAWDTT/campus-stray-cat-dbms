import {useEffect,useState,useCallback} from 'react'
import { useNavigate } from 'react-router-dom'
import {Table,type TableColumn,Button,Icon,Notification,Input} from 'animal-island-ui'
import {PageHeader} from '../../../shared/components/PageHeader'
import {VolunteerService} from '../../../services/volunteer.service'

const loadData=async ()=>{
  return await VolunteerService.getPendingApplications()
}

export function AdoptionCheckPage({ embedded = false }: { embedded?: boolean }) {
  const navigate = useNavigate()
  const [data,setData]=useState<any[]>([])
  const [query,setQuery]=useState('')

  const refreshData = useCallback(() => {
    loadData().then(setData).catch(() => {
      Notification.error('数据加载失败')
    })
  }, [])

  useEffect(()=>{
    loadData().then((data)=>{
      setData(data)
    }).catch(()=>{
      Notification.error('数据加载失败')
    })
  },[])

  const handleDecision = (applicationId:string, status:'APPROVED'|'REJECTED') => {
    VolunteerService.checkAdoption(applicationId,status).then(()=>{
      Notification.success(status === 'APPROVED' ? '已同意领养申请' : '已拒绝领养申请')
      refreshData()
    }).catch(()=> Notification.error('审核失败，请稍后重试'))
  }

  const AdoptionColumns:TableColumn<any>[]=[
  {
    title:'申请编号',
    dataIndex:'applicationId'
  },{
    title:'猫咪id',
    dataIndex:'catId'
  },{
    title:'猫咪名称',
    dataIndex:'catName'
  },{
    title:'申请人id',
    dataIndex:'applicantUserId'
  },{
    title:'申请人名称',
    dataIndex:'applicantName'
  },{
    title:'申请时间',
    dataIndex:'applyTime',
    render: (text: any) => <span>{text ? text.format('YYYY-MM-DD HH:mm') : '-'}</span>
  },{
    title:'操作',
    render: (_: any, value: any) => (
      <div className="adoption-decision-row">
        <Button className="adoption-decision-approve" type='primary' size='small' onClick={() => handleDecision(String(value?.applicationId ?? ''),'APPROVED')}>同意</Button>
        <Button className="adoption-decision-reject" type='default' size='small' onClick={() => handleDecision(String(value?.applicationId ?? ''),'REJECTED')}>拒绝</Button>
      </div>
    )
  }
]
  const filteredData = data.filter((item:any) => {
    const text = `${item.applicationId ?? ''} ${item.catName ?? ''} ${item.applicantName ?? ''} ${item.applicantUserId ?? ''}`.toLowerCase()
    return text.includes(query.trim().toLowerCase())
  })
  const dateText = (value:any) => value ? value.format('YYYY-MM-DD HH:mm') : '-'
  return (
    <>
      <div className='adoption-check-page'>
        {!embedded && <div className='adoption-check-page-header'>
          <PageHeader kicker='Adoption Check' title="领养审核" description='处理待审核的领养申请' icon='icon-design'
            actions={<Button type="text" size="small" onClick={() => navigate('/volunteer')}><Icon name="icon-miles" size={15} />返回</Button>} />
        </div>}
        <div className='adoption-check-page-table'>
          <div style={{display:'flex',justifyContent:'flex-end',marginBottom:12}}>
            <Input value={query} onChange={(e:any)=>setQuery(e.target.value)} placeholder='搜索申请编号、猫咪或申请人' style={{maxWidth:280}} />
          </div>
          <div className="mobile-adoption-review-list" aria-label="待审核领养申请">
            {filteredData.length === 0 ? <div className="mobile-adoption-empty">没有待审核的领养申请</div> : filteredData.map((item:any) => <article className="mobile-adoption-review-card" key={String(item.applicationId)}>
              <div><strong>{item.catName || '猫咪信息待补充'}</strong><small>申请人：{item.applicantName || item.applicantUserId || '未知'} · {dateText(item.applyTime)}</small></div>
              <div className="adoption-decision-row">
                <Button className="adoption-decision-approve" type="primary" size="small" onClick={() => handleDecision(String(item.applicationId || ''), 'APPROVED')}>同意</Button>
                <Button className="adoption-decision-reject" type="default" size="small" onClick={() => handleDecision(String(item.applicationId || ''), 'REJECTED')}>拒绝</Button>
              </div>
            </article>)}
          </div>
          <div className="adoption-table-wrap">
            <Table columns={AdoptionColumns} dataSource={filteredData} />
          </div>
        </div>
      </div>
    </>
  );
}
