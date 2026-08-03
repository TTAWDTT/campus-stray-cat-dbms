# D组 task_17_18_19_oracle_programming 接口文档

本文档对应 [database/queries/task_17_18_19_oracle_programming.sql](../../database/queries/task_17_18_19_oracle_programming.sql)。

## 一、领养流程接口

### 1. 获取待审核领养申请

接口说明：根据状态获取待审核领养申请

HTTP URL：`http://localhost:3000/api/adoption-workflow/pending`

HTTP Method：GET

请求参数：无

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | `[{"applicationId":"APP001","catId":"C001","catName":"Mimi","applicantUserId":"U001","applicantName":"Tom","applyTime":"2026-08-01T10:20:30","currentStatus":"PENDING","reviewerUserId":null,"agreementNo":null,"confirmTime":null}]` |

### 2. 获取领养回访汇总

接口说明：获取领养回访记录列表

HTTP URL：`http://localhost:3000/api/adoption-workflow/visits`

HTTP Method：GET

请求参数：无

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | `[{"visitId":"VIS001","applicationId":"APP001","catId":"C001","visitType":"PHONE","visitTime":"2026-08-01T14:20:30","visitorUserId":"U002","conclusion":"状态正常","passFlag":1,"currentStatus":"APPROVED"}]` |

### 3. 新建领养申请

接口说明：新建领养申请

HTTP URL：`http://localhost:3000/api/adoption-workflow/applications`

HTTP Method：POST

请求体：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| catId | string | 是 | 猫咪编号 |
| applicantUserId | string | 否 | 服务端从当前登录用户获取，客户端传入值会被忽略 |
| status | string | 否 | 服务端固定为 `PENDING` |

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 创建成功 | `""` |

### 4. 审核领养申请

接口说明：根据申请编号审核领养申请

HTTP URL：`http://localhost:3000/api/adoption-workflow/applications/{applicationId}/review`

HTTP Method：POST

请求参数：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| applicationId | string | 是 | 申请编号 |

请求体：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| reviewerUserId | string | 否 | 服务端从当前登录用户获取 |
| status | string | 是 | 只能是 `APPROVED` 或 `REJECTED` |
| agreementNo | string | 否 | 协议编号 |
| confirmTime | DateTime | 否 | 确认时间 |

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 204 | 审核成功 | 无 |

### 5. 新建回访记录

接口说明：为领养申请新建回访记录

HTTP URL：`http://localhost:3000/api/adoption-workflow/applications/{applicationId}/visits`

HTTP Method：POST

请求参数：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| applicationId | string | 是 | 申请编号 |

请求体：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| visitType | string | 是 | 回访类型 |
| visitTime | DateTime | 否 | 回访时间 |
| visitorUserId | string | 否 | 服务端从当前登录用户获取 |
| conclusion | string | 否 | 回访结论 |
| passFlag | int | 否 | 是否通过，默认 `0` |

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 创建成功 | `""` |

## 二、志愿者流程接口

### 6. 获取志愿者看板数据

接口说明：获取志愿者活动汇总

HTTP URL：`http://localhost:3000/api/volunteer-workflow/activity`

HTTP Method：GET

请求参数：无

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | `[{"volunteerId":"V001","userId":"U001","userName":"Tom","activeStatus":"ACTIVE","creditLevel":"L1","serviceScore":10,"shiftId":"S001","shiftStatus":"PLANNED","planStartTime":"2026-08-01T08:00:00","planEndTime":"2026-08-01T12:00:00"}]` |

### 7. 新增志愿者

接口说明：注册志愿者

HTTP URL：`http://localhost:3000/api/volunteer-workflow/volunteers`

HTTP Method：POST

请求体：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| userId | string | 是 | 用户编号 |
| joinDate | DateTime | 否 | 加入时间 |
| serviceScore | number | 否 | 服务积分，默认 `0` |
| creditLevel | string | 否 | 信用等级，默认 `L1` |
| activeStatus | string | 否 | 状态，默认 `ACTIVE` |
| graduationYear | string | 否 | 毕业年份 |

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 创建成功 | `""` |

### 8. 新建排班

接口说明：创建志愿者排班

HTTP URL：`http://localhost:3000/api/volunteer-workflow/shifts`

HTTP Method：POST

请求体：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| volunteerId | string | 是 | 志愿者编号 |
| pointId | string | 是 | 值守点编号 |
| backupVolunteerId | string | 否 | 备班志愿者编号 |
| planStartTime | DateTime | 是 | 计划开始时间 |
| planEndTime | DateTime | 是 | 计划结束时间 |
| shiftStatus | string | 否 | 排班状态，默认 `PLANNED` |

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 创建成功 | `""` |

### 9. 签到排班

接口说明：根据排班编号进行签到

HTTP URL：`http://localhost:3000/api/volunteer-workflow/shifts/{shiftId}/checkins`

HTTP Method：POST

请求参数：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| shiftId | string | 是 | 排班编号 |

请求体：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| checkInTime | DateTime | 否 | 签到时间 |
| longitude | number | 否 | 经度 |
| latitude | number | 否 | 纬度 |
| photoUrl | string | 否 | 照片地址 |
| distanceMeters | number | 否 | 距离米数 |
| checkInStatus | string | 否 | 签到状态，默认 `CHECKED_IN` |

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 签到成功 | `""` |

签到成功后，系统会自动增加 1 分服务积分并写入 `VOL_CREDITLOGS`，同一排班不能重复签到。

### 10. 新增积分变更记录

接口说明：新增志愿者积分日志

HTTP URL：`http://localhost:3000/api/volunteer-workflow/credit-logs`

HTTP Method：POST

请求体：

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| volunteerId | string | 是 | 志愿者编号 |
| sourceType | string | 是 | 来源类型 |
| sourceId | string | 是 | 来源编号 |
| scoreChange | number | 是 | 积分变化值 |
| creditLevelAfter | string | 是 | 变更后信用等级 |
| createTime | DateTime | 否 | 创建时间 |
| remark | string | 否 | 备注 |

响应体：

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 创建成功 | `""` |
