/**
 * 志愿者 / 领养流程模块 — 模拟数据
 *
 * 数据与后端 AdoptionPendingAppDto / AdoptionVisitSummaryDto 结构一致，
 * 经 volunteer.service.ts 中的 toApplication / toVisit 清洗后使用。
 *
 * 将 USE_MOCK 设为 true 即可使用此数据预览，设为 false 则调用真实 API。
 */

/** Mock 开关 — 统一设为 true 或 false 即可切换所有页面的数据来源 */
export const USE_MOCK = true

type ApiRecord = Record<string, unknown>

// ═══════════════════════════════════════════
// 1. 待审核领养申请
// ═══════════════════════════════════════════
export const mockApplications: ApiRecord[] = [
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

// ═══════════════════════════════════════════
// 1b. 已通过领养申请（用于新建回访记录）
// ═══════════════════════════════════════════
export const mockApprovedApplications: ApiRecord[] = [
    {
        applicationId: 'APP24004',
        catId: 'CAT004',
        catName: '小白',
        applicantUserId: 'U1004',
        applicantName: '赵六',
        applyTime: '2026-08-01T10:00:00',
        currentStatus: 'APPROVED',
        reviewerUserId: 'U2001',
        agreementNo: 'AGR2026001',
        confirmTime: '2026-08-05T15:00:00',
    },
    {
        applicationId: 'APP24005',
        catId: 'CAT005',
        catName: '花花',
        applicantUserId: 'U1005',
        applicantName: '孙七',
        applyTime: '2026-08-03T09:30:00',
        currentStatus: 'APPROVED',
        reviewerUserId: 'U2002',
        agreementNo: 'AGR2026002',
        confirmTime: '2026-08-06T11:00:00',
    },
]

// ═══════════════════════════════════════════
// 2. 回访记录
// ═══════════════════════════════════════════
export const mockVisits: ApiRecord[] = [
    {
        visitId: 'VIS001',
        applicationId: 'APP24001',
        catId: 'CAT001',
        visitType: 'INITIAL',
        visitTime: '2026-08-07T09:00:00',
        visitorUserId: 'U2001',
        conclusion: '申请人住所稳定，已封窗，具备养猫条件',
        passFlag: 1,
        currentStatus: 'APPROVED',
    },
    {
        visitId: 'VIS002',
        applicationId: 'APP24002',
        catId: 'CAT002',
        visitType: 'INITIAL',
        visitTime: '2026-08-08T14:30:00',
        visitorUserId: 'U2002',
        conclusion: '合租房东不允许养宠物，建议暂缓',
        passFlag: 0,
        currentStatus: 'REJECTED',
    },
    {
        visitId: 'VIS003',
        applicationId: 'APP24001',
        catId: 'CAT001',
        visitType: 'FOLLOW_UP',
        visitTime: '2026-08-09T10:00:00',
        visitorUserId: 'U2001',
        conclusion: '猫咪已适应新环境，状态良好，饮食正常',
        passFlag: 1,
        currentStatus: 'APPROVED',
    },
]

// ═══════════════════════════════════════════
// 3. 志愿者活动（排班）数据
// ═══════════════════════════════════════════
export const mockActivities: ApiRecord[] = [
    {
        volunteerId: 'V001',
        userId: 'U2001',
        userName: '李四',
        activeStatus: 'ACTIVE',
        creditLevel: 'L2',
        serviceScore: 120.5,
        shiftId: 'SHIFT001',
        shiftStatus: 'IN_PROGRESS',
        planStartTime: '2026-08-09T08:00:00',
        planEndTime: '2026-08-09T16:00:00',
    },
    {
        volunteerId: 'V001',
        userId: 'U2001',
        userName: '李四',
        activeStatus: 'ACTIVE',
        creditLevel: 'L2',
        serviceScore: 120.5,
        shiftId: 'SHIFT003',
        shiftStatus: 'ASSIGNED',
        planStartTime: '2026-08-10T09:00:00',
        planEndTime: '2026-08-10T12:00:00',
    },
    {
        volunteerId: 'V001',
        userId: 'U2001',
        userName: '李四',
        activeStatus: 'ACTIVE',
        creditLevel: 'L2',
        serviceScore: 120.5,
        shiftId: 'SHIFT004',
        shiftStatus: 'PLANNED',
        planStartTime: '2026-08-12T08:00:00',
        planEndTime: '2026-08-12T16:00:00',
    },
    {
        volunteerId: 'V002',
        userId: 'U2002',
        userName: '王五',
        activeStatus: 'ACTIVE',
        creditLevel: 'L1',
        serviceScore: 45.0,
        shiftId: 'SHIFT002',
        shiftStatus: 'COMPLETED',
        planStartTime: '2026-08-08T10:00:00',
        planEndTime: '2026-08-08T14:00:00',
    },
    {
        volunteerId: 'V003',
        userId: 'U2003',
        userName: '赵六',
        activeStatus: 'INACTIVE',
        creditLevel: 'L1',
        serviceScore: 12.0,
        shiftId: 'SHIFT005',
        shiftStatus: 'MISSED',
        planStartTime: '2026-08-07T08:00:00',
        planEndTime: '2026-08-07T12:00:00',
    },
]

// ═══════════════════════════════════════════
// 4. 投喂任务数据（对应 VolShift 实体，字段 PascalCase + ID 后缀）
// ═══════════════════════════════════════════
export const mockFeedingTasks: ApiRecord[] = [
    {
        ShiftID: 'SHIFT001',
        VolunteerID: 'V001',
        PointID: 'P001',
        BackupVolunteerID: null,
        PlanStartTime: '2026-08-09T08:00:00',
        PlanEndTime: '2026-08-09T16:00:00',
        ShiftStatus: 'IN_PROGRESS',
    },
    {
        ShiftID: 'SHIFT002',
        VolunteerID: 'V002',
        PointID: 'P002',
        BackupVolunteerID: 'V001',
        PlanStartTime: '2026-08-08T10:00:00',
        PlanEndTime: '2026-08-08T14:00:00',
        ShiftStatus: 'COMPLETED',
    },
    {
        ShiftID: 'SHIFT003',
        VolunteerID: 'V001',
        PointID: 'P001',
        BackupVolunteerID: null,
        PlanStartTime: '2026-08-10T09:00:00',
        PlanEndTime: '2026-08-10T12:00:00',
        ShiftStatus: 'ASSIGNED',
    },
    {
        ShiftID: 'SHIFT004',
        VolunteerID: 'V001',
        PointID: 'P003',
        BackupVolunteerID: null,
        PlanStartTime: '2026-08-12T08:00:00',
        PlanEndTime: '2026-08-12T16:00:00',
        ShiftStatus: 'PLANNED',
    },
    {
        ShiftID: 'SHIFT005',
        VolunteerID: 'V003',
        PointID: 'P002',
        BackupVolunteerID: null,
        PlanStartTime: '2026-08-07T08:00:00',
        PlanEndTime: '2026-08-07T12:00:00',
        ShiftStatus: 'MISSED',
    },
]

// ═══════════════════════════════════════════
// 5. 投喂记录（打卡记录，对应 VolCheckIn）
// ═══════════════════════════════════════════
export const mockFeedingRecords: ApiRecord[] = [
    {
        CheckInID: 'CI001',
        ShiftID: 'SHIFT001',
        CheckInTime: '2026-08-09T08:15:00',
        Longitude: 121.5064,
        Latitude: 31.2457,
        PhotoUrl: 'https://example.com/photos/feed-ci001.jpg',
        DistanceMeters: 5.2,
        CheckInStatus: 'CHECKED_IN',
    },
    {
        CheckInID: 'CI002',
        ShiftID: 'SHIFT002',
        CheckInTime: '2026-08-08T10:20:00',
        Longitude: 121.5068,
        Latitude: 31.2460,
        PhotoUrl: 'https://example.com/photos/feed-ci002.jpg',
        DistanceMeters: 12.8,
        CheckInStatus: 'CHECKED_IN',
    },
    {
        CheckInID: 'CI003',
        ShiftID: 'SHIFT003',
        CheckInTime: '2026-08-10T09:35:00',
        Longitude: 121.5070,
        Latitude: 31.2463,
        PhotoUrl: '',
        DistanceMeters: 0,
        CheckInStatus: 'LATE',
    },
]

// ═══════════════════════════════════════════
// 6. 交接记录（对应 VolHandover）
// ═══════════════════════════════════════════
export const mockHandovers: ApiRecord[] = [
    {
        HandoverID: 'HO001',
        FromVolunteerID: 'V001',
        ToVolunteerID: 'V002',
        HandoverType: 'TASK_TRANSFER',
        RelatedType: 'SHIFT',
        RelatedID: 'SHIFT001',
        ApplyTime: '2026-08-09T12:00:00',
        ConfirmTime: null,
        HandoverStatus: 'PENDING',
        Remark: '临时有事，麻烦帮忙喂一下',
    },
    {
        HandoverID: 'HO002',
        FromVolunteerID: 'V002',
        ToVolunteerID: 'V001',
        HandoverType: 'TASK_TRANSFER',
        RelatedType: 'SHIFT',
        RelatedID: 'SHIFT002',
        ApplyTime: '2026-08-08T09:00:00',
        ConfirmTime: '2026-08-08T09:30:00',
        HandoverStatus: 'CONFIRMED',
        Remark: '已确认，下午我去喂',
    },
    {
        HandoverID: 'HO003',
        FromVolunteerID: 'V001',
        ToVolunteerID: 'V003',
        HandoverType: 'TASK_TRANSFER',
        RelatedType: 'SHIFT',
        RelatedID: 'SHIFT003',
        ApplyTime: '2026-08-10T08:00:00',
        ConfirmTime: null,
        HandoverStatus: 'REJECTED',
        Remark: '时间冲突无法接替',
    },
]
