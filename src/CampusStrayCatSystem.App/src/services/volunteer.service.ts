import {http} from './http'
import dayjs from 'dayjs'

// 切换为 true 使用本地 mock 数据，联调后端时改回 false
const mock_check = true

type ApiRecord=Record<string,unknown>

const value=<T>(data:ApiRecord,camel:string,pascal:string):T|undefined=>
  (data[camel]??data[pascal]) as T|undefined

const toApplication=(data:ApiRecord)=>{
    const rawApplyTime = value<string>(data,'applyTime','ApplyTime')
    const rawConfirmTime = value<string>(data,'confirmTime','ConfirmTime')
    return {
        applicationId:value<string>(data,'applicationId','ApplicationId')||'',
        catId:value<string>(data,'catId','CatId')||'',
        catName:value<string>(data,'catName','CatName')||'',
        applicantUserId:value<string>(data,'applicantUserId','ApplicantUserId')||'',
        applicantName:value<string>(data,'applicantName','ApplicantName')||'',
        applyTime: rawApplyTime ? dayjs(rawApplyTime) : undefined,
        currentStatus:value<string>(data,'currentStatus','CurrentStatus')||'',
        reviewerUserId:value<string>(data,'reviewerUserId','ReviewerUserId')||'',
        agreementNo:value<string>(data,'agreementNo','AgreementNo')||'',
        confirmTime: rawConfirmTime ? dayjs(rawConfirmTime) : undefined,
    }
}

// ---- mock 数据（原始 API 格式，与后端返回结构一致）----
let mockApiData: ApiRecord[] = [
    {
        applicationId: 'APP24001',
        catId: 'CAT001',
        catName: '小橘',
        applicantUserId: 'U1001',
        applicantName: '张三',
        applyTime: '2026-08-06T10:30:00',
        currentStatus: 'PENDING',
        reviewerUserId: '',
        agreementNo: '',
        confirmTime: null,
    },
    {
        applicationId: 'APP24002',
        catId: 'CAT002',
        catName: '大花',
        applicantUserId: 'U1002',
        applicantName: '李四',
        applyTime: '2026-08-07T14:00:00',
        currentStatus: 'PENDING',
        reviewerUserId: '',
        agreementNo: '',
        confirmTime: null,
    },
    {
        applicationId: 'APP24003',
        catId: 'CAT003',
        catName: '黑尾',
        applicantUserId: 'U1003',
        applicantName: '王五',
        applyTime: '2026-08-08T09:15:00',
        currentStatus: 'PENDING',
        reviewerUserId: '',
        agreementNo: '',
        confirmTime: null,
    },
]

export const VolunteerService={
    //获取待审核申请列表
    async getPendingApplications(){
        if (mock_check) {
            return mockApiData.map(toApplication)
        }
        const {data}=await http.get('/adoption-workflow/pending')
        return (data as ApiRecord[]).map(toApplication)
    },
    //审核领养申请
    async checkAdoption(applicationId:string,checkStatus:string){
        if (mock_check) {
            // 模拟后端行为：审核后从待审核列表中移除
            mockApiData = mockApiData.filter(item =>
                item.applicationId !== applicationId && item.ApplicationId !== applicationId
            )
            console.log(`[mock] 审核申请 ${applicationId}，结果: ${checkStatus}，剩余 ${mockApiData.length} 条待审核`)
            return
        }
        const payload={
            status:checkStatus,
            confirmTime:dayjs().toISOString()
        }
        await http.post(
            `/adoption-workflow/applications/${encodeURIComponent(applicationId)}/review`,
            payload
        )
    }
}