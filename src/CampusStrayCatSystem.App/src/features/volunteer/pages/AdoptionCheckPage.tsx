import {useEffect,useState,useCallback} from 'react'
import { useNavigate } from 'react-router-dom'
import {Table,type TableColumn,Button,Icon,Notification, Radio,Modal} from 'animal-island-ui'
import {PageHeader} from '../../../shared/components/PageHeader'
import {VolunteerService} from '../../../services/volunteer.service'

const CheckStatus=[
  {label:'通过',value:'APPROVED'},
  {label:'拒绝',value:'REJECTED'}
]

const loadData=async ()=>{
  return await VolunteerService.getPendingApplications()
}

export function AdoptionCheckPage() {
  const navigate = useNavigate()
  const [modalOpen,setModalOpen]=useState(false)
  const [selectedId,setSelectedId]=useState('')
  const [checkStatus,setCheckStatus]=useState('APPROVED')
  const [data,setData]=useState<any[]>([])

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

  const handleOk = () => {
    VolunteerService.checkAdoption(selectedId,checkStatus).then(()=>{
      Notification.success('审核成功')
      setModalOpen(false)
      refreshData()
    }).catch(()=>{
      Notification.error('审核失败')
    })
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
      <div>
        <Button type='primary' danger onClick={() => { setSelectedId(String(value?.applicationId ?? '')); setModalOpen(true); }}>审核</Button>
      </div>
    )
  }
]
  return (
    <>
      <div className='adoption-check-page'>
        <div className='adoption-check-page-header'>
          <PageHeader kicker='Adoption Check' title="领养审核" description='处理待审核的领养申请' icon='icon-design'
            actions={<Button type="text" size="small" onClick={() => navigate('/volunteer')}><Icon name="icon-miles" size={15} />返回</Button>} />
        </div>
        <div className='adoption-check-page-table'>
          <Table columns={AdoptionColumns} dataSource={data} />
        </div>
        <div className='adoption-check-page-modal'>
          <Modal title='审核领养申请' open={modalOpen} onClose={() => setModalOpen(false)} onOk={handleOk}>
            <span>请选择审核结果:</span>
            <Radio options={CheckStatus} value={checkStatus} onChange={(value) => setCheckStatus(String(value))}></Radio>
          </Modal>
        </div>
      </div>
    </>
  );
}