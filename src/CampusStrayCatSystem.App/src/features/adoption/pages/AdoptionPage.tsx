import {Table,Button,type TableColumn,Notification,Modal,Card,Icon} from 'animal-island-ui'
import type {CatSummary} from '../../../types/cats'
import {adoptionService} from '../../../services/adoption.services'
import {useEffect,useState} from 'react'
import {StatusTag} from '../../../shared/components/StatusTag'
import {PageHeader} from '../../../shared/components/PageHeader'
import {USE_MOCK, MOCK_CATS} from '../../adoption/test'
import { useAuthStore } from '../../../stores/auth.store'
function handleAdopt(catID:string,userID:string){
    adoptionService.postAdoption(catID,userID).then(()=>{
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
export function AdoptionPage(){
    const [cats,setCats]=useState<CatSummary[]>([])
    const [loading,setLoading]=useState(false)
    const [visible,setVisible]=useState(false)
    const [CatID,setCatID]=useState<string>('')
    const user=useAuthStore((s)=>s.user)
    const userId=user?.userId
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
                </div>
                <Modal
                    title='确认领养'
                    open={visible}
                    onOk={() => {
                        handleAdopt(CatID, userId as string);
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