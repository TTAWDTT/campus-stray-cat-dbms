import {http} from './http'
import dayjs from 'dayjs'
import { USE_MOCK, mockApplications, mockVisits, mockActivities } from '../features/volunteer/test/mockData'

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
const toVisit=(data:ApiRecord)=>{
    const rawVisitTime=value<string>(data,'visitTime','VisitTime')
    return {
        visitId:value<string>(data,'visitId','VisitId')||'',
        applicationId:value<string>(data,'applicationId','ApplicationId')||'',
        catId:value<string>(data,'catId','CatId')||'',
        visitType:value<string>(data,'visitType','VisitType')||'',
        visitTime: rawVisitTime ? dayjs(rawVisitTime) : undefined,
        visitorUserId:value<string>(data,'visitorUserId','VisitorUserId')||'',
        conclusion:value<string>(data,'conclusion','Conclusion')||'',
        passFlag:value<number>(data,'passFlag','PassFlag')||0,
        currentStatus:value<string>(data,'currentStatus','CurrentStatus')||'',
    }
}

const toActivity=(data:ApiRecord)=>{
    const rawPlanStartTime = value<string>(data,'planStartTime','PlanStartTime')
    const rawPlanEndTime = value<string>(data,'planEndTime','PlanEndTime')
    return {
        volunteerId:value<string>(data,'volunteerId','VolunteerId')||'',
        userId:value<string>(data,'userId','UserId')||'',
        userName:value<string>(data,'userName','UserName')||'',
        activeStatus:value<string>(data,'activeStatus','ActiveStatus')||'',
        creditLevel:value<string>(data,'creditLevel','CreditLevel')||'',
        serviceScore:value<number>(data,'serviceScore','ServiceScore')||0,
        shiftId:value<string>(data,'shiftId','ShiftId')||'',
        shiftStatus:value<string>(data,'shiftStatus','ShiftStatus')||'',
        planStartTime: rawPlanStartTime ? dayjs(rawPlanStartTime) : undefined,
        planEndTime: rawPlanEndTime ? dayjs(rawPlanEndTime) : undefined,
    }
}

export const shiftStatusLabels: Record<string, string> = {
    PLANNED: '计划',
    ASSIGNED: '已分配',
    IN_PROGRESS: '执行中',
    COMPLETED: '已完成',
    MISSED: '逾期',
}

export const visitTypeLabels: Record<string, string> = {
    INITIAL: '初次回访',
    FOLLOW_UP: '跟进回访',
    FINAL: '最终回访',
}

export const passFlagLabels: Record<number, string> = {
    0: '未通过',
    1: '通过',
}

export const VolunteerService={
    //获取待审核申请列表
    async getPendingApplications(){
        if (USE_MOCK) {
            return mockApplications.map(toApplication)
        }
        const {data}=await http.get('/adoption-workflow/pending')
        return (data as ApiRecord[]).map(toApplication)
    },
    //获取回访列表
    async getVisitList(){
        if (USE_MOCK) {
            return mockVisits.map(toVisit)
        }
        const {data}=await http.get('/adoption-workflow/visits')
        return (data as ApiRecord[]).map(toVisit)
    },
    //获取志愿者活动（排班）列表
    async getActivity(){
        if (USE_MOCK) {
            return mockActivities.map(toActivity)
        }
        const {data}=await http.get('/volunteer-workflow/activity')
        return (data as ApiRecord[]).map(toActivity)
    },
    //签到
    async checkInShift(shiftId: string, payload: Record<string, unknown>) {
        if (USE_MOCK) {
            console.log(`[mock] 签到排班 ${shiftId}`, payload)
            return
        }
        await http.post(
            `/volunteer-workflow/shifts/${encodeURIComponent(shiftId)}/checkins`,
            payload
        )
    },
    //获取已审核申请列表
    //审核领养申请
    async checkAdoption(applicationId:string,checkStatus:string){
        if (USE_MOCK) {
            // 模拟后端行为：审核后从待审核列表中移除
            const idx = mockApplications.findIndex(item =>
                item.applicationId === applicationId || item.ApplicationId === applicationId
            )
            if (idx >= 0) mockApplications.splice(idx, 1)
            console.log(`[mock] 审核申请 ${applicationId}，结果: ${checkStatus}，剩余 ${mockApplications.length} 条待审核`)
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