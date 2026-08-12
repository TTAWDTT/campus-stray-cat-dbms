import {Table,Button,type TableColumn,Notification,Modal,Card,Icon} from 'animal-island-ui'
import type {CatSummary} from '../../../types/cats'
import {adoptionService,applicationStatusLabels} from '../../../services/adoption.services'
import {useEffect,useState} from 'react'
import {StatusTag} from '../../../shared/components/StatusTag'
import {PageHeader} from '../../../shared/components/PageHeader'
import {USE_MOCK, MOCK_CATS} from '../../adoption/test'

function handleAdopt(catID:string){
    adoptionService.postAdoption(catID).then(()=>{
        Notification.success('领养申请已提交，请等待管理员审核')
    }).catch(()=>{
        Notification.error('提交领养申请失败，请稍后再试')
    })
}
async function loadCats() {
  if (USE_MOCK) {
    return MOCK_CATS
  }
  try {
    const cats = await adoptionService.getOnCampusCats();
    return cats;
  } catch (error) {
    console.error('加载猫咪失败:', error);
    throw error;
  }
}

const statusTagMap: Record<string, {value: string; label: string}> = {
    PENDING: {value: 'PENDING', label: '待审核'},
    APPROVED: {value: 'COMPLETED', label: '已通过'},
    REJECTED: {value: 'REJECTED', label: '已拒绝'},
}

export function AdoptionPage(){
    const [cats,setCats]=useState<CatSummary[]>([])
    const [loading,setLoading]=useState(false)
    const [visible,setVisible]=useState(false)
    const [CatID,setCatID]=useState<string>('')
    const [applications, setApplications] = useState<any[]>([])

    useEffect(()=>{
        setLoading(true)
        loadCats().then(data=>{
            setCats(data)
        }).catch(err=>{
            console.error('加载猫咪失败:',err)
        }).finally(()=>{
            setLoading(false)
        })
    },[])

    useEffect(()=>{
        adoptionService.getMyApplications().then(setApplications).catch(()=>setApplications([]))
    },[])

    const CatTableColumn:TableColumn[]=[
    {
        title:'猫咪id',
        dataIndex:'catID'
    },{
        title:'猫咪昵称',
        dataIndex:'catName'
    },{
        title:'性别',
        dataIndex:'gender'
    },{
        title:'花色',
        dataIndex:'colorPattern'
    },{
        title:'绝育',
        dataIndex:'sterilizedFlag',
        render:(_,record)=>(
        <StatusTag value={record.sterilizedFlag==0?'app-green':'app-red'} label={record.sterilizedFlag==0?'已绝育':'未绝育'}/>
    )
    },{
        title:'领养',
        render:(_,record)=>{
            const recordCatID = record.catID as string;
            return <Button type='default' size='small' onClick={()=>{setCatID(recordCatID);setVisible(true)}}>就是它了</Button>
        }
    }
    ]

    const applicationColumns: TableColumn[] = [
        { title: '申请编号', dataIndex: 'applicationId' },
        { title: '猫咪', dataIndex: 'catName' },
        { title: '申请时间', dataIndex: 'applyTime', render: (t: any) => t ? t.format('YYYY-MM-DD HH:mm') : '-' },
        {
            title: '状态', dataIndex: 'currentStatus',
            render: (t: any) => {
                const item = statusTagMap[t]
                return item ? <StatusTag value={item.value} label={item.label} /> : <span>{applicationStatusLabels[t] || t || '-'}</span>
            },
        },
        { title: '协议号', dataIndex: 'agreementNo', render: (t: any) => t || '-' },
        { title: '审核时间', dataIndex: 'confirmTime', render: (t: any) => t ? t.format('YYYY-MM-DD HH:mm') : '-' },
    ]

    return (
        <>
            <div className='user-page'>
                <div className='page-header'>
                    <PageHeader kicker='Cat On Campus' title='在校猫咪列表' description='选择一只心仪的猫咪，给它一个家' icon='icon-critterpedia'></PageHeader>
                </div>
                <div className='page-content'>
                    <Card className='adoption-tip' style={{marginBottom:16}}>
                        <div className='adoption-tip-body'>
                            <Icon name='icon-chat' size={20} className='adoption-tip-icon' />
                            <div>
                                <strong>领养不是一时兴起</strong>
                                <p>每一只猫咪的生命约有十五载，领养是一份长久的承诺。请在确认有稳定的居住环境、充足的经济能力和家人的支持后，再做出决定。</p>
                            </div>
                        </div>
                    </Card>
                    <Table columns={CatTableColumn} dataSource={cats as unknown as Record<string, unknown>[]} loading={loading}></Table>

                    {applications.length > 0 && (
                        <div style={{ marginTop: 24 }}>
                            <h3 style={{ marginBottom: 12 }}>我的申请</h3>
                            <Table columns={applicationColumns} dataSource={applications} />
                        </div>
                    )}
                </div>
                <Modal
                    title='确认领养'
                    open={visible}
                    onOk={() => {
                        handleAdopt(CatID);
                        setVisible(false);
                    }}
                    onClose={()=>setVisible(false)}
                >
                    <p>您确定要领养这只猫咪吗？</p>
                </Modal>
            </div>
        </>
    )
}