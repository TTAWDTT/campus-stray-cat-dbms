# D 组（功能点 17-21）：领养、志愿者、投喂与财务接口测试文档

## 1. 公共约定

| 项目 | 约定 |
|---|---|
| 基础地址 | `http://localhost:5047` |
| 认证方式 | 受保护接口携带 `Authorization: Bearer <token>`；公开查询和具体角色见各接口 |
| 数据格式 | JSON |
| 时间格式 | ISO 8601 |
| 领养状态 | `PENDING`、`APPROVED`、`REJECTED` |
| 排班状态 | `PLANNED`、`ASSIGNED`、`COMPLETED` 等数据库约定值 |
| 交接状态 | `PENDING`、`CONFIRMED`、`REJECTED`、`CANCELLED` |
| 项目状态 | `ACTIVE`、`COMPLETED`、`CANCELLED` |
| 支出审核状态 | `PENDING`、`APPROVED`、`REJECTED` |

### 字段编码字典

接口中的枚举值统一使用英文编码，中文只作为前端显示文本。

| 字段 | 合法值 | 中文含义 |
|---|---|---|
| `status`（领养申请） | `PENDING`、`APPROVED`、`REJECTED` | 待审核、已通过、已驳回 |
| `visitType` | `INITIAL`、`FOLLOW_UP`、`FINAL` | 首次、跟进、最终回访 |
| `passFlag`、`publicFlag` | `0`、`1` | 否、是 |
| `creditLevel` | `L1`、`L2`、`L3` | 积分等级一、二、三 |
| `activeStatus` | `ACTIVE`、`INACTIVE` | 启用、停用 |
| `shiftStatus` | `PLANNED`、`ASSIGNED`、`IN_PROGRESS`、`COMPLETED`、`MISSED` | 计划、已分配、进行中、已完成、漏签 |
| `checkInStatus` | `CHECKED_IN`、`LATE` | 已签到、迟到 |
| `handoverType` | `TASK`、`SHIFT` | 任务、排班 |
| `relatedType` | `SHIFT` | 排班关联（当前固定值） |
| `handoverStatus` | `PENDING`、`CONFIRMED`、`REJECTED`、`CANCELLED` | 待确认、已确认、已拒绝、已撤销 |
| `projectStatus` | `ACTIVE`、`COMPLETED`、`CANCELLED` | 进行中、已完成、已取消 |
| `payMethod` | `ALIPAY`、`WECHAT`、`BANK_TRANSFER`、`CASH`、`OTHER` | 支付宝、微信、银行转账、现金、其他 |
| `recordType`（支出） | `FOOD`、`MEDICAL`、`SUPPLIES`、`OTHER` | 食物、医疗、物资、其他 |
| `auditStatus` | `PENDING`、`APPROVED`、`REJECTED` | 待审核、已通过、已驳回 |
| `metricCode` | `TOTAL_DONATION`、`TOTAL_EXPENSE`、`NET_BALANCE`、`DONATION_COUNT` | 捐赠总额、支出总额、净余额、捐赠笔数 |
| `dimensionType` | `PROJECT`、`MONTH`、`CAT` | 项目、月份、猫咪 |
| `unit` | `CNY`、`COUNT` | 人民币、数量 |

金额字段（如 `amount`、`targetAmount`）不得为负；`planEndTime` 必须晚于 `planStartTime`。

## 2. 接口总览

| 功能 | 方法 | 基础路径 |
|---|---|---|
| 领养申请与回访 | GET/POST | `/api/adoption-workflow` |
| 志愿者流程 | GET/POST | `/api/volunteer-workflow` |
| 投喂任务 | GET/POST/PUT | `/api/feeding-tasks` |
| 投喂记录 | GET/POST | `/api/feeding-records` |
| 交接记录 | GET/POST/PUT | `/api/handovers` |
| 众筹项目 | GET/POST/PUT | `/api/crowdfunding-projects` |
| 捐赠 | GET/POST | `/api/donations` |
| 支出 | GET/POST/PUT | `/api/expense-records` |
| 财务公示 | GET | `/api/financial-disclosure` |
| 统计快照 | GET/POST | `/api/statistics-reports` |

## 3. 领养流程

### 请求：获取领养申请或回访汇总

| 接口说明 | 查询待审核申请、指定状态的领养申请或回访汇总 |
|---|---|
| HTTP URL | `http://localhost:5047/api/adoption-workflow/pending`；`/applications?status=APPROVED`；`/visits` |
| HTTP Method | `GET` |
| 权限要求 | 管理员或志愿者 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | 申请 DTO 数组或回访汇总数组 |
| 401/403 | 未授权或角色不足 | 错误信息 |
| 400 | `status` 不是 `PENDING`、`APPROVED` 或 `REJECTED` | 错误信息 |

`/pending` 保留为待审核列表；`/applications` 默认查询 `APPROVED`，可通过 `status` 查询 `PENDING`、`APPROVED` 或 `REJECTED`。新增回访前，前端应调用 `/applications?status=APPROVED` 取得申请编号。

### 请求：提交领养申请

| 接口说明 | 普通用户为猫咪提交领养申请；申请人从 JWT 获取 |
|---|---|
| HTTP URL | `http://localhost:5047/api/adoption-workflow/applications` |
| HTTP Method | `POST` |
| 权限要求 | 已登录 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `catId` | string | 是 | 猫咪 ID |
| `applicantUserId` | string | 否 | 忽略客户端值，以当前登录用户为准 |
| `status` | string | 否 | 忽略客户端值，服务端使用 `PENDING` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 提交成功 | 当前实现为空响应体；申请 ID 需从数据库或审核列表确认 |
| 400 | 猫咪 ID 为空 | 错误信息 |
| 401 | 未登录 | 未授权 |
| 500 | 数据库约束或关联数据失败 | 错误信息 |

### 请求：审核申请

| 接口说明 | 管理员或志愿者通过或驳回领养申请 |
|---|---|
| HTTP URL | `http://localhost:5047/api/adoption-workflow/applications/{applicationId}/review` |
| HTTP Method | `POST` |
| 权限要求 | 管理员或志愿者 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `status` | string | 是 | `APPROVED` 或 `REJECTED` |
| `reviewerUserId` | string | 否 | 以当前登录用户为准 |
| `agreementNo` | string | 否 | 协议编号 |
| `confirmTime` | DateTime | 否 | 确认时间 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 审核成功 | 无 |
| 400 | 申请 ID、请求体或状态非法 | 错误信息 |
| 401/403 | 未授权或角色不足 | 错误信息 |
| 500 | 申请不存在、黑名单或数据库业务错误 | 错误信息 |

### 请求：新增回访记录

| 接口说明 | 为已审核申请记录回访 |
|---|---|
| HTTP URL | `http://localhost:5047/api/adoption-workflow/applications/{applicationId}/visits` |
| HTTP Method | `POST` |
| 权限要求 | 管理员或志愿者 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `visitType` | string | 是 | 回访类型 |
| `visitTime` | DateTime | 否 | 回访时间 |
| `visitorUserId` | string | 否 | 以当前登录用户为准 |
| `conclusion` | string | 否 | 回访结论 |
| `passFlag` | int | 否 | `0` 或 `1`，默认 `0` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 创建成功 | 空响应体 |
| 400 | 申请 ID、回访类型或 passFlag 非法 | 错误信息 |
| 401/403 | 未授权或角色不足 | 错误信息 |

## 4. 志愿者流程

### 请求：获取志愿者看板

| 接口说明 | 查询志愿者活动、排班和积分汇总 |
|---|---|
| HTTP URL | `http://localhost:5047/api/volunteer-workflow/activity` |
| HTTP Method | `GET` |
| 权限要求 | 管理员或志愿者 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `VolunteerActivityDto[]` |
| 401/403 | 未授权或角色不足 | 错误信息 |

### 请求：注册志愿者

| 接口说明 | 将系统用户注册为志愿者 |
|---|---|
| HTTP URL | `http://localhost:5047/api/volunteer-workflow/volunteers` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `userId` | string | 是 | 系统用户 ID |
| `joinDate` | DateTime | 否 | 加入时间 |
| `serviceScore` | decimal | 否 | 默认 0 |
| `creditLevel` | string | 否 | 默认 `L1` |
| `activeStatus` | string | 否 | 默认 `ACTIVE` |
| `graduationYear` | string | 否 | 毕业年份 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 注册成功 | 空响应体 |
| 400 | 用户 ID 为空 | 错误信息 |
| 409 | 已注册或唯一约束冲突 | 错误信息 |

### 请求：新建排班

| 接口说明 | 为志愿者创建投喂点排班 |
|---|---|
| HTTP URL | `http://localhost:5047/api/volunteer-workflow/shifts` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `volunteerId` | string | 是 | 有效志愿者 ID |
| `pointId` | string | 是 | 服务点 ID |
| `backupVolunteerId` | string | 否 | 不得与负责人相同 |
| `planStartTime` | DateTime | 是 | 开始时间 |
| `planEndTime` | DateTime | 是 | 必须晚于开始时间 |
| `shiftStatus` | string | 否 | 默认 `PLANNED` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 创建成功 | 空响应体 |
| 400 | 参数或时间非法 | 错误信息 |
| 409 | 志愿者、点位或备用志愿者无效 | 错误信息 |

### 请求：签到排班

| 接口说明 | 志愿者签到；成功后排班完成并增加服务积分 |
|---|---|
| HTTP URL | `http://localhost:5047/api/volunteer-workflow/shifts/{shiftId}/checkins` |
| HTTP Method | `POST` |
| 权限要求 | 志愿者 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `checkInTime` | DateTime | 否 | 签到时间 |
| `longitude`/`latitude` | decimal | 否 | 坐标 |
| `photoUrl` | string | 否 | 照片 URL |
| `distanceMeters` | decimal | 否 | 距离 |
| `checkInStatus` | string | 否 | 默认 `CHECKED_IN` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 签到成功 | 空响应体 |
| 400 | 排班或请求体为空 | 错误信息 |
| 409 | 排班已签到 | 错误信息 |

### 请求：新增积分日志

| 接口说明 | 新增志愿者积分变更记录 |
|---|---|
| HTTP URL | `http://localhost:5047/api/volunteer-workflow/credit-logs` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 请求体

`volunteerId`、`sourceType`、`sourceId`、`scoreChange`、`creditLevelAfter` 必填；`createTime`、`remark` 可选。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 创建成功 | 空响应体 |
| 400 | 必填字段缺失 | 错误信息 |

## 5. 投喂任务、签到与交接

### 请求：查询投喂任务或投喂记录

| 接口说明 | 查询全部、按 ID、志愿者、点位、状态或排班查询 |
|---|---|
| HTTP URL | `/api/feeding-tasks`、`/by-volunteer/{volunteerId}`、`/by-point/{pointId}`、`/by-status/{status}`；`/api/feeding-records`、`/{id}`、`/by-shift/{shiftId}`、`/by-volunteer/{volunteerId}` |
| HTTP Method | `GET` |
| 权限要求 | 管理员或志愿者；“我的”查询只能查询本人数据 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `VolShift[]` 或 `VolCheckIn[]` |
| 403 | 查询他人“我的”数据 | 无权限 |
| 404 | 对象不存在 | 错误信息 |

### 请求：新增或更新投喂任务

| 接口说明 | 创建或修改投喂排班 |
|---|---|
| HTTP URL | `http://localhost:5047/api/feeding-tasks` 或 `/api/feeding-tasks/{id}` |
| HTTP Method | `POST` 或 `PUT` |
| 权限要求 | 管理员；负责人可按控制器规则更新 |

#### 请求体

字段：`volunteerID`、`pointID`、`backupVolunteerID`、`planStartTime`、`planEndTime`、`shiftStatus`；创建时 ID 由后端生成。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | `VolShift` |
| 204 | 更新成功 | 无 |
| 400 | 时间、志愿者或点位非法 | 错误信息 |
| 403 | 无权操作他人任务 | 无权限 |
| 409 | 数据库操作未生效 | 错误信息 |

### 请求：更新投喂任务状态

| 接口说明 | 更新任务状态 |
|---|---|
| HTTP URL | `http://localhost:5047/api/feeding-tasks/{id}/status` |
| HTTP Method | `PUT` |
| 权限要求 | 管理员或任务负责人 |

#### 请求体

```json
{"newStatus":"COMPLETED"}
```

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 更新成功 | message |
| 403 | 无权操作 | 无权限 |
| 404 | 任务不存在 | 错误信息 |

### 请求：提交投喂签到记录

| 接口说明 | 新增签到记录并在事务中完成投喂任务 |
|---|---|
| HTTP URL | `http://localhost:5047/api/feeding-records` |
| HTTP Method | `POST` |
| 权限要求 | 管理员或志愿者 |

#### 请求体

字段：`shiftID`、`checkInTime`、`longitude`、`latitude`、`photoUrl`、`distanceMeters`、`checkInStatus`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | `VolCheckIn` |
| 400 | 排班或字段非法 | 错误信息 |
| 403 | 非任务负责人 | 无权限 |
| 409 | 重复签到或写入未生效 | 错误信息 |

### 请求：查询或提交交接

| 接口说明 | 查询交接历史，或由当前任务负责人发起交接 |
|---|---|
| HTTP URL | `/api/handovers`、`/{id}`、`/by-from/{fromVolunteerId}`、`/by-to/{toVolunteerId}`、`/by-status/{status}`、`/by-related/{relatedType}/{relatedId}` |
| HTTP Method | `GET` 或 `POST` |
| 权限要求 | 管理员或志愿者；发起交接需为任务负责人 |

#### 请求体（POST）

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `fromVolunteerID` | string | 是 | 发起方志愿者 |
| `toVolunteerID` | string | 是 | 接收方志愿者，不能相同 |
| `handoverType` | string | 否 | 交接类型 |
| `relatedType` | string | 是 | 当前必须为 `SHIFT` |
| `relatedID` | string | 是 | 投喂任务 ID |
| `remark` | string | 否 | 备注 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | 交接记录数组 |
| 201 | 提交成功 | `VolHandover` |
| 400 | 志愿者、任务或状态非法 | 错误信息 |
| 403 | 非任务负责人 | 无权限 |
| 404 | 关联对象不存在 | 错误信息 |

### 请求：确认、拒绝或撤销交接

| 接口说明 | 接收方确认/拒绝，发起方撤销 |
|---|---|
| HTTP URL | `http://localhost:5047/api/handovers/{id}/confirm`、`/reject`、`/cancel` |
| HTTP Method | `PUT` |
| 权限要求 | 确认/拒绝为接收方；撤销为发起方；管理员可操作 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 操作成功 | message |
| 400 | 当前状态不是 `PENDING` | 错误信息 |
| 403 | 当前用户不是对应志愿者 | 无权限 |
| 404 | 交接不存在 | 错误信息 |
| 409 | 状态已被其他请求改变 | 错误信息 |

## 6. 众筹、捐赠与财务公示

### 请求：查询或维护众筹项目

| 接口说明 | 查询项目，或创建、编辑、更新项目状态 |
|---|---|
| HTTP URL | `/api/crowdfunding-projects`、`/{id}`、`/by-status/{status}`、`/by-cat/{catId}`、`/{id}/status` |
| HTTP Method | `GET`、`POST` 或 `PUT` |
| 权限要求 | 查询公开；创建、编辑和状态更新仅管理员 |

#### 请求体

创建/更新字段：`catID`、`title`、`targetAmount`、`startTime`、`endTime`、`projectStatus`；`raisedAmount` 由系统维护，创建时归零。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询或状态更新成功 | 项目对象或 message |
| 201 | 创建成功 | 项目对象 |
| 400 | 金额、时间或状态非法 | 错误信息 |
| 404 | 项目或猫咪不存在 | 错误信息 |
| 409 | 数据库操作未生效 | 错误信息 |

### 请求：查询或记录捐赠

| 接口说明 | 查询捐赠或在事务中新增捐赠并累加项目金额 |
|---|---|
| HTTP URL | `/api/donations`、`/{id}`、`/by-project/{projectId}`、`/by-donor/{donorUserId}` |
| HTTP Method | `GET` 或 `POST` |
| 权限要求 | 管理员可查全部；本人可查本人捐赠；创建需登录 |

#### 请求体（POST）

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `projectID` | string | 是 | ACTIVE 众筹项目 ID |
| `donorUserID` | string | 否 | 当前实现允许指定已存在用户，联调时应使用本人 ID |
| `amount` | decimal | 是 | 正数 |
| `payMethod` | string | 否 | 支付方式 |
| `payTime` | DateTime | 否 | 支付时间 |
| `publicFlag` | int | 否 | `0` 匿名、`1` 公开 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | 捐赠数组 |
| 201 | 创建成功 | 捐赠对象 |
| 400 | 项目、金额或公开标记非法 | 错误信息 |
| 403 | 查询他人捐赠 | 无权限 |
| 404 | 项目或捐赠人不存在 | 错误信息 |
| 409 | 项目状态变化导致写入未生效 | 错误信息 |

### 请求：查询、记录或审核支出

| 接口说明 | 查询支出、记录支出、审核支出 |
|---|---|
| HTTP URL | `/api/expense-records`、`/{id}`、`/by-project/{projectId}`、`/by-project/{projectId}/approved-expenses`、`/{id}/audit` |
| HTTP Method | `GET`、`POST` 或 `PUT` |
| 权限要求 | 查询按控制器权限；审核需管理员 |

#### 请求体

记录支出字段：`projectID`、`recordType`、`amount`、`invoiceUrl`。服务端强制 `auditStatus=PENDING`，清空审核人和公示时间。

审核字段：`auditStatus` 只能为 `APPROVED` 或 `REJECTED`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功或审核成功 | 支出数组、对象或 message |
| 201 | 创建成功 | 支出对象 |
| 400 | 金额、项目或审核状态非法 | 错误信息 |
| 404 | 支出或项目不存在 | 错误信息 |
| 409 | 审核状态已变化或写入未生效 | 错误信息 |

### 请求：查询财务公示

| 接口说明 | 查询指定项目明细或所有进行中项目摘要 |
|---|---|
| HTTP URL | `http://localhost:5047/api/financial-disclosure/{projectId}` 或 `/api/financial-disclosure/summary` |
| HTTP Method | `GET` |
| 权限要求 | 公开 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `FinancialDisclosureDto` 或摘要数组 |
| 404 | 项目不存在 | 错误信息 |

### 请求：查询或生成统计快照

| 接口说明 | 查询统计快照，或为指定项目生成统计数据 |
|---|---|
| HTTP URL | `/api/statistics-reports`、`/snapshot/{id}`、`/by-metric/{metricCode}`、`/by-dimension/{dimensionType}/{dimensionValue}`、`/generate/{projectId}` |
| HTTP Method | `GET` 或 `POST` |
| 权限要求 | 查询需登录；生成快照仅管理员 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `RptStatisticsSnapshot[]` 或对象 |
| 201 | 快照生成成功 | 新生成的快照 |
| 404 | 快照或项目不存在 | 错误信息 |
| 409 | 事务生成未生效 | 错误信息 |

## 7. 本地联调

1. 执行 `database/setup_all.sql`。
2. 使用管理员、志愿者和普通用户分别获取 Token。
3. 测试顺序建议为：领养申请 → 审核/回访；志愿者注册 → 排班 → 签到；投喂 → 交接；众筹 → 捐赠 → 支出审核 → 财务公示 → 统计快照。
4. 重点验证普通用户不能审核领养、非本人不能查询“我的”志愿者/捐赠数据、重复签到和非法外键被拒绝。
