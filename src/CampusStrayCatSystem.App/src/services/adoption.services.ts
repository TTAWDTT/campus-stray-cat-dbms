import {http} from './http'
import dayjs from 'dayjs'
import {toCat} from './cats.service'
import { USE_MOCK, MOCK_APPLICATIONS } from '../features/adoption/test'

type ApiRecord=Record<string,unknown>

const value=<T>(data:ApiRecord,camel:string,pascal:string):T|undefined=>
  (data[camel]??data[pascal]) as T|undefined

const toApplication=(data:ApiRecord)=>{
    const rawApplyTime=value<string>(data,'applyTime','ApplyTime')
    const rawConfirmTime=value<string>(data,'confirmTime','ConfirmTime')
    return {
        applicationId:value<string>(data,'applicationId','ApplicationId')||'',
        catId:value<string>(data,'catId','CatId')||'',
        catName:value<string>(data,'catName','CatName')||'',
        applicantUserId:value<string>(data,'applicantUserId','ApplicantUserId')||'',
        applicantName:value<string>(data,'applicantName','ApplicantName')||'',
        applyTime:rawApplyTime?dayjs(rawApplyTime):undefined,
        currentStatus:value<string>(data,'currentStatus','CurrentStatus')||'',
        reviewerUserId:value<string>(data,'reviewerUserId','ReviewerUserId')||'',
        agreementNo:value<string>(data,'agreementNo','AgreementNo')||'',
        confirmTime:rawConfirmTime?dayjs(rawConfirmTime):undefined,
    }
}

export const applicationStatusLabels: Record<string, string> = {
    PENDING: '待审核',
    APPROVED: '已通过',
    REJECTED: '已拒绝',
}

export const adoptionService={
    async getOnCampusCats(){
        const {data}=await http.get('/cat',{params:{lifeStatus:'ON_CAMPUS'}})
        return data.map(toCat)
    },
    async postAdoption(catID:string,applicationUserID:string){
        await http.post('/adoption',{params:{CatID:catID,applicationUserID:applicationUserID,status:'PENDING'}})
    },
    async getMyApplications(){
        if (USE_MOCK) return MOCK_APPLICATIONS.map(toApplication)
        const {data}=await http.get('/adoption-workflow/my-applications')
        return (data as ApiRecord[]).map(toApplication)
    }
}