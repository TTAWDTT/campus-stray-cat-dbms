# C 组（功能点 11-16）：救助、TNR 与医疗接口测试文档

## 1. 公共约定

| 项目 | 约定 |
|---|---|
| 基础地址 | `http://localhost:5047` |
| 认证方式 | 受保护接口携带 `Authorization: Bearer <token>` |
| 时间格式 | ISO 8601 |
| TNR 状态 | `DISCOVERED`、`CAPTURED`、`SURGERY`、`RECOVERING`、`RELEASED`、`CANCELLED` |
| 提醒状态 | `PENDING`、`SENT`、`COMPLETED` |
| 紧急等级 | `LOW`、`MEDIUM`、`HIGH`、`CRITICAL` |
| 上报状态 | `SUBMITTED`、`ASSIGNED`、`PROCESSING`、`RESOLVED`、`CLOSED` |
| 失踪预警状态 | `PROCESSING`、`FOUND`、`CLOSED` |

### 字段编码字典

接口中的状态和类型统一使用英文编码，前端再映射为中文显示。

| 字段 | 合法值 | 中文含义 |
|---|---|---|
| `currentStatus`、`fromStatus`、`toStatus` | `DISCOVERED`、`CAPTURED`、`SURGERY`、`RECOVERING`、`RELEASED`、`CANCELLED` | 发现、捕捉、手术、恢复、放归、取消 |
| `recordType`、`reminderType` | `VACCINATION`、`CHECKUP`、`TREATMENT`、`SURGERY`、`DEWORMING`、`EMERGENCY`、`OTHER` | 疫苗、检查、治疗、手术、驱虫、紧急、其他 |
| `sendStatus` | `PENDING`、`SENT`、`COMPLETED` | 待发送、已发送、已完成 |
| `animalType` | `CAT`、`DOG`、`OTHER` | 猫、狗、其他 |
| `urgencyLevel` | `LOW`、`MEDIUM`、`HIGH`、`CRITICAL` | 低、中、高、紧急 |
| `processStatus` | `SUBMITTED`、`ASSIGNED`、`PROCESSING`、`RESOLVED`、`CLOSED` | 已提交、已分配、处理中、已解决、已关闭 |
| `alertStatus` | `PROCESSING`、`FOUND`、`CLOSED` | 处理中、已寻回、已关闭 |

金额字段（如 `totalCost`）不得为负；`thresholdDays` 必须大于 0；捕捉、手术、放归时间应按流程先后填写。

TNR、TNR 状态日志、医疗记录和医疗提醒接口要求 `ADMIN`、`VOLUNTEER` 或 `VET` 角色。紧急上报和失踪预警的查询、创建要求登录；分配或更新处理状态要求 `ADMIN` 或 `VOLUNTEER`。

## 2. 接口总览

| 功能 | 方法 | URL |
|---|---|---|
| TNR 案例 | GET/POST/PUT | `/api/TnrCases...` |
| TNR 状态日志 | GET | `/api/TnrStatusLogs/case/{caseId}` |
| 医疗记录 | GET/POST/PUT | `/api/MedHealthRecords...` |
| 医疗提醒 | GET/POST/PUT | `/api/MedReminder...` |
| 紧急上报 | GET/POST/PUT | `/api/EmergencyReports...` |
| 失踪预警 | GET/POST/PUT | `/api/MissingAlerts...` |

`TnrCases`、`MedReminder` 等路径来自 ASP.NET `[controller]` 路由，URL 大小写不敏感；本文保留代码中的实际路径写法。

## 3. TNR 案例与状态日志

### 请求：查询 TNR 案例

| 接口说明 | 查询全部或单个 TNR 案例 |
|---|---|
| HTTP URL | `http://localhost:5047/api/TnrCases` 或 `/api/TnrCases/{id}` |
| HTTP Method | `GET` |
| 权限要求 | 管理员、志愿者或兽医 |

#### 请求参数

单条查询需要路径参数 `id`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `TnrCase[]` 或 `TnrCase` |
| 404 | 案例不存在 | 错误信息 |

### 请求：创建 TNR 案例

| 接口说明 | 创建捕捉、绝育和放归流程案例 |
|---|---|
| HTTP URL | `http://localhost:5047/api/TnrCases` |
| HTTP Method | `POST` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `catID` | string | 是 | 猫咪 ID |
| `responsibleUserID` | string | 否 | 负责人用户 ID |
| `currentStatus` | string | 否 | TNR 状态 |
| `hospitalName` | string | 否 | 医院名称 |
| `captureTime` | DateTime | 否 | 捕捉时间 |
| `surgeryTime` | DateTime | 否 | 手术时间 |
| `releaseTime` | DateTime | 否 | 放归时间 |
| `totalCost` | decimal | 否 | 总费用，不得为负 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | TNR 案例对象 |
| 400 | 猫咪、状态或金额非法 | 错误信息 |
| 409 | 数据库写入未生效 | 错误信息 |

### 请求：更新 TNR 案例基本信息

| 接口说明 | 更新医院、时间、费用等基本信息 |
|---|---|
| HTTP URL | `http://localhost:5047/api/TnrCases/{id}` |
| HTTP Method | `PUT` |

#### 请求体

使用创建接口字段，并确保 URL 中的 `id` 与请求体 `caseID` 一致（如请求体提供）。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无 |
| 400 | 参数非法 | 错误信息 |
| 404 | 案例不存在 | 错误信息 |
| 409 | 更新未生效 | 错误信息 |

### 请求：更新 TNR 状态

| 接口说明 | 更新当前状态并生成状态流转日志 |
|---|---|
| HTTP URL | `http://localhost:5047/api/TnrCases/{id}/status` |
| HTTP Method | `PUT` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `newStatus` | string | 是 | 目标 TNR 状态 |
| `operatorID` | string | 否 | 操作人；优先取当前登录用户 |
| `remark` | string | 否 | 处理说明 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 更新成功 | 状态变化结果 |
| 400 | 状态非法或流程不允许 | 错误信息 |
| 404 | 案例不存在 | 错误信息 |

### 请求：查询 TNR 状态日志

| 接口说明 | 查看某个案例的完整状态流转 |
|---|---|
| HTTP URL | `http://localhost:5047/api/TnrStatusLogs/case/{caseId}` |
| HTTP Method | `GET` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `TnrStatusLog[]` |
| 404 | 案例不存在 | 错误信息 |

## 4. 医疗健康记录

### 请求：查询医疗记录

| 接口说明 | 查询全部、按猫咪查询或按 ID 查询医疗历史 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MedHealthRecords`、`/cat/{catId}`、`/{id}` |
| HTTP Method | `GET` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `MedHealthRecord[]` 或单条记录 |
| 404 | 记录或猫咪不存在 | 错误信息 |

### 请求：新增医疗记录

| 接口说明 | 记录就诊、疾病、用药、绝育和疫苗等信息 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MedHealthRecords` |
| HTTP Method | `POST` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `catID` | string | 是 | 猫咪 ID |
| `recordType` | string | 是 | 记录类型 |
| `hospitalName` | string | 否 | 医院名称 |
| `diagnosis` | string | 否 | 诊断或用药说明 |
| `recordDate` | DateTime | 否 | 记录时间 |
| `nextDueDate` | DateTime | 否 | 下次处理时间 |
| `attachmentUrl` | string | 否 | 附件 URL |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | 医疗记录对象 |
| 400 | 猫咪或记录类型非法 | 错误信息 |
| 409 | 写入未生效 | 错误信息 |

### 请求：编辑医疗记录

| 接口说明 | 修改医疗记录 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MedHealthRecords/{id}` |
| HTTP Method | `PUT` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无 |
| 400 | 参数非法 | 错误信息 |
| 404 | 记录不存在 | 错误信息 |
| 409 | 更新未生效 | 错误信息 |

## 5. 医疗提醒

### 请求：查询医疗提醒

| 接口说明 | 查询待处理提醒、按猫咪查询历史或按 ID 查询详情 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MedReminder`、`/cat/{catId}`、`/{reminderId}` |
| HTTP Method | `GET` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `MedReminder[]` 或提醒对象 |
| 404 | 提醒不存在 | 错误信息 |

### 请求：新增医疗提醒

| 接口说明 | 创建疫苗、驱虫、手术、复查等后续提醒 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MedReminder` |
| HTTP Method | `POST` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `recordID` | string | 否 | 医疗记录 ID；如提供必须属于 `catID` |
| `catID` | string | 是 | 猫咪 ID |
| `reminderType` | string | 是 | `VACCINATION`、`CHECKUP`、`TREATMENT`、`SURGERY`、`DEWORMING`、`EMERGENCY`、`OTHER` |
| `receiverUserID` | string | 否 | 接收提醒的用户 |
| `reminderTime` | DateTime | 是 | 提醒时间 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | `sendStatus` 为 `PENDING` 的提醒对象 |
| 400 | 猫咪、记录、类型或时间非法 | 错误信息 |
| 404 | 关联对象不存在 | 错误信息 |

### 请求：标记提醒为已发送或已完成

| 接口说明 | 更新提醒状态 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MedReminder/{reminderId}/sent` 或 `/complete` |
| HTTP Method | `PUT` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无 |
| 404 | 提醒不存在 | 错误信息 |

## 6. 紧急救助上报

### 请求：查询紧急上报

| 接口说明 | 查询全部或单条紧急救助上报 |
|---|---|
| HTTP URL | `http://localhost:5047/api/EmergencyReports` 或 `/api/EmergencyReports/{reportId}` |
| HTTP Method | `GET` |
| 权限要求 | 已登录；上报内容含用户和处理信息 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `EmergencyReport[]` 或对象 |
| 404 | 上报不存在 | 错误信息 |

### 请求：提交紧急上报

| 接口说明 | 提交受伤、被困或疑似生病的救助报告 |
|---|---|
| HTTP URL | `http://localhost:5047/api/EmergencyReports` |
| HTTP Method | `POST` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `areaID` | string | 是 | 校园区域 ID |
| `animalType` | string | 是 | 动物类型，如 `CAT` |
| `photoURL` | string | 否 | 图片 URL |
| `longitude`/`latitude` | decimal | 否 | 坐标 |
| `urgencyLevel` | string | 是 | `LOW`、`MEDIUM`、`HIGH`、`CRITICAL` |
| `reporterUserID` | string | 否 | 忽略客户端值，以当前登录用户为准 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | `processStatus=SUBMITTED` 的上报对象 |
| 400 | 区域、动物类型或紧急等级非法 | 错误信息 |
| 404 | 区域不存在 | 错误信息 |

### 请求：分配处理人

| 接口说明 | 为上报分配处理人 |
|---|---|
| HTTP URL | `http://localhost:5047/api/EmergencyReports/{reportId}/assign` |
| HTTP Method | `PUT` |

#### 请求体

```json
"user-002"
```

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 分配成功 | 无 |
| 404 | 上报或处理人不存在 | 错误信息 |

### 请求：更新上报处理状态

| 接口说明 | 更新处理状态和处理结果；管理员或当前处理人可操作 |
|---|---|
| HTTP URL | `http://localhost:5047/api/EmergencyReports/{reportId}/status` |
| HTTP Method | `PUT` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `processStatus` | string | 是 | `SUBMITTED`、`ASSIGNED`、`PROCESSING`、`RESOLVED`、`CLOSED` |
| `processResult` | string | 否 | 处理结果 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无 |
| 400 | 状态非法 | 错误信息 |
| 403 | 非管理员且不是当前处理人 | 无权限 |
| 404 | 上报不存在 | 错误信息 |

## 7. 猫咪失踪预警

### 请求：查询失踪预警

| 接口说明 | 查询全部、按猫咪或按 ID 查询失踪预警 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MissingAlerts`、`/cat/{catId}`、`/{alertId}` |
| HTTP Method | `GET` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `CatMissingAlert[]` 或对象 |
| 404 | 预警不存在 | 错误信息 |

### 请求：创建失踪预警

| 接口说明 | 创建处理中失踪预警；同一猫只能有一个处理中预警 |
|---|---|
| HTTP URL | `http://localhost:5047/api/MissingAlerts` |
| HTTP Method | `POST` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `catID` | string | 是 | 猫咪 ID |
| `lastSightingID` | string | 否 | 最后目击记录，必须属于该猫 |
| `lastSightingTime` | DateTime | 否 | 最后目击时间 |
| `thresholdDays` | int | 否 | 必须大于 0 |
| `handlerUserID` | string | 否 | 创建时由服务端清空；处理人通过分配接口设置 |
| `remark` | string | 否 | 备注 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | `alertStatus=PROCESSING` 的预警对象 |
| 400 | 猫咪或阈值非法 | 错误信息 |
| 404 | 猫咪或最后目击不存在 | 错误信息 |
| 409 | 已存在处理中预警 | 错误信息 |

### 请求：更新失踪预警状态

| 接口说明 | 更新为 `PROCESSING`、`FOUND` 或 `CLOSED` |
|---|---|
| HTTP URL | `http://localhost:5047/api/MissingAlerts/{alertId}/status` |
| HTTP Method | `PUT` |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `alertStatus` | string | 是 | `PROCESSING`、`FOUND`、`CLOSED` |
| `handlerUserID` | string | 是 | 处理人 ID |
| `remark` | string | 否 | 处理备注 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无 |
| 400 | 状态或处理人非法 | 错误信息 |
| 404 | 预警不存在 | 错误信息 |

## 8. 本地联调

1. 执行 `database/setup_all.sql`。
2. 先准备猫咪、校园区域和用户演示数据。
3. 按 TNR → 医疗记录 → 医疗提醒 → 紧急上报 → 失踪预警顺序测试。
4. 重点验证非法外键返回 4xx、TNR 状态日志生成、同猫重复处理中预警被拒绝。
