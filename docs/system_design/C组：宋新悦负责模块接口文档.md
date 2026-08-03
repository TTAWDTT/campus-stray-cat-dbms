# C 组：功能 11-13 接口文档

> **总 URL 基址**：`http://localhost:5047/api`

---

## 一、TNR 救助案例接口

涉及文件：`TnrCasesController.cs`

### 请求：获取所有 TNR 案例

| 接口说明 | 获取全部 TNR 救助案例列表 |
| ----------- | ------------------ |
| HTTP URL | TnrCases |
| HTTP Method | GET |

#### 请求参数

无

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | [\{<br>"caseID": "3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"responsibleUserID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"currentStatus": "CAPTURED",<br>"hospitalName": "同济大学附属宠物医院",<br>"captureTime": "2026-08-01T10:30:00",<br>"surgeryTime": "2026-08-02T14:00:00",<br>"releaseTime": null,<br>"totalCost": 500.00<br>\}] |

### 请求：根据 ID 获取单条 TNR 案例

| 接口说明 | 根据案例 ID 获取单条 TNR 救助案例的详细信息 |
| ----------- | --------------------------- |
| HTTP URL | TnrCases/\{id\} |
| HTTP Method | GET |

#### 请求参数

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| id | string | 是 | TNR 案例 ID，示例值："3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c" |

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | \{<br>"caseID": "3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"responsibleUserID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"currentStatus": "DISCOVERED",<br>"hospitalName": "同济大学附属宠物医院",<br>"captureTime": "2026-08-01T10:30:00",<br>"surgeryTime": null,<br>"releaseTime": null,<br>"totalCost": 300.00<br>\} |
| 404 | 未找到 | "未找到 ID 为 3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c 的TNR案例。" |

### 请求：创建 TNR 案例

| 接口说明 | 新建一个 TNR 救助案例，记录需要救助的猫咪、负责人、医院、时间和费用等信息 |
| ----------- | --------------------------------------------- |
| HTTP URL | TnrCases |
| HTTP Method | POST |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| caseID | string | 否 | 案例 ID，后端自动生成 |
| catID | string | 是 | 猫咪 ID，不能为空，必须在 CAT_CATS 表中存在 |
| responsibleUserID | string | 否 | 负责人用户 ID，必须在 SYS_USERS 表中存在 |
| currentStatus | string | 否 | 当前状态，必须为 `DISCOVERED`、`CAPTURED`、`SURGERY`、`RECOVERING`、`RELEASED`、`CANCELLED` 之一 |
| hospitalName | string | 否 | 合作医院名称 |
| captureTime | DateTime | 否 | 计划捕捉时间 |
| surgeryTime | DateTime | 否 | 计划手术时间 |
| releaseTime | DateTime | 否 | 计划放归时间 |
| totalCost | decimal | 否 | 总费用，不能为负数 |

#### 请求示例

\{
<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",
<br>"responsibleUserID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",
<br>"currentStatus": "DISCOVERED",
<br>"hospitalName": "同济大学附属宠物医院",
<br>"captureTime": "2026-08-03T09:00:00",
<br>"surgeryTime": "2026-08-04T14:00:00",
<br>"totalCost": 500.00
<br>\}

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 201 | 创建成功 | \{<br>"caseID": "6f2c7f6a-0f4b-4c2b-9f2d-1c3f8f7d3d61",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"responsibleUserID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"currentStatus": "DISCOVERED",<br>"hospitalName": "同济大学附属宠物医院",<br>"captureTime": "2026-08-03T09:00:00",<br>"surgeryTime": "2026-08-04T14:00:00",<br>"releaseTime": null,<br>"totalCost": 500.00<br>\} |
| 400 | 参数不合法 | "TNR案例数据为空，无法创建。" |
| 400 | 业务校验失败 | "CatID 不能为空。"<br>"猫咪 CatID='xxx' 不存在。"<br>"负责人 UserID='xxx' 不存在。"<br>"无效的状态值 'xxx'。允许的状态: DISCOVERED, CAPTURED, SURGERY, RECOVERING, RELEASED, CANCELLED"<br>"TotalCost 不能为负数。"<br>"手术时间不能早于捕获时间。"<br>"释放时间不能早于手术时间。"<br>"释放时间不能早于捕获时间。" |

### 请求：更新 TNR 案例基本信息

| 接口说明 | 更新已有 TNR 案例的基本信息（猫咪、负责人、医院、时间、费用等），仅更新字段值，不生成状态流转日志 |
| ----------- | -------------------------------------------------------- |
| HTTP URL | TnrCases/\{id\} |
| HTTP Method | PUT |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| caseID | string | 是 | 案例 ID，必须与 URL 中的 ID 一致 |
| catID | string | 是 | 猫咪 ID，不能为空，必须在 CAT_CATS 表中存在 |
| responsibleUserID | string | 否 | 负责人用户 ID，必须在 SYS_USERS 表中存在 |
| currentStatus | string | 否 | 当前状态（直接覆盖，如需记录流转请使用状态更新接口） |
| hospitalName | string | 否 | 合作医院名称 |
| captureTime | DateTime | 否 | 捕捉时间 |
| surgeryTime | DateTime | 否 | 手术时间 |
| releaseTime | DateTime | 否 | 放归时间 |
| totalCost | decimal | 否 | 总费用，不能为负数 |

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 204 | 更新成功 | 无 |
| 400 | 参数不合法 | "TNR案例数据为空，无法更新。"<br>"URL 中的 ID 与请求体中的 ID 不匹配。" |
| 400 | 业务校验失败 | 同创建接口的校验错误信息 |
| 404 | 未找到 | "未找到 ID 为 xxx 的TNR案例，无法更新。" |

### 请求：更新 TNR 状态

| 接口说明 | 更新 TNR 案例的当前状态，系统在同一数据库事务中自动生成状态流转日志 |
| ----------- | -------------------------------------------- |
| HTTP URL | TnrCases/\{id\}/status |
| HTTP Method | PUT |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| newStatus | string | 是 | 新状态值，必须为 `DISCOVERED`、`CAPTURED`、`SURGERY`、`RECOVERING`、`RELEASED`、`CANCELLED` 之一 |
| operatorID | string | 否 | 操作人用户 ID |
| remark | string | 否 | 处理说明 / 备注 |

#### 请求示例

\{
<br>"newStatus": "SURGERY",
<br>"operatorID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",
<br>"remark": "开始绝育手术"
<br>\}

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 更新成功 | \{<br>"oldStatus": "CAPTURED",<br>"newStatus": "SURGERY",<br>"message": "状态更新成功，已生成流转日志。"<br>\} |
| 400 | 参数不合法 | "新状态不能为空。"<br>"无效的状态值 'xxx'。允许的状态: DISCOVERED, CAPTURED, SURGERY, RECOVERING, RELEASED, CANCELLED" |
| 404 | 未找到 | "未找到 ID 为 xxx 的TNR案例，无法更新状态。" |

---

## 二、TNR 状态流转记录接口

涉及文件：`TnrStatusLogsController.cs`

### 请求：查看某个案例的完整状态流转记录

| 接口说明 | 根据 TNR 案例 ID 查询该案例从发现到放归（或取消）的完整状态流转日志 |
| ----------- | -------------------------------------------- |
| HTTP URL | TnrStatusLogs/case/\{caseId\} |
| HTTP Method | GET |

#### 请求参数

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| caseId | string | 是 | TNR 案例 ID，示例值："3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c" |

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | [\{<br>"logID": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",<br>"caseID": "3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c",<br>"fromStatus": "DISCOVERED",<br>"toStatus": "CAPTURED",<br>"operatorID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"opTime": "2026-08-01T10:30:00",<br>"remark": "在图书馆后方成功捕捉，已送往医院"<br>\},<br>\{<br>"logID": "b2c3d4e5-f6a7-8901-bcde-f12345678901",<br>"caseID": "3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c",<br>"fromStatus": "CAPTURED",<br>"toStatus": "SURGERY",<br>"operatorID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"opTime": "2026-08-02T14:00:00",<br>"remark": "开始绝育手术"<br>\},<br>\{<br>"logID": "c3d4e5f6-a7b8-9012-cdef-123456789012",<br>"caseID": "3f8a7b2c-1d4e-5f6a-8b9c-0d1e2f3a4b5c",<br>"fromStatus": "SURGERY",<br>"toStatus": "RECOVERING",<br>"operatorID": "u1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"opTime": "2026-08-02T16:30:00",<br>"remark": "手术完成，转入恢复观察"<br>\}] |
| 404 | 未找到 | "未找到 ID 为 xxx 的TNR案例。" |

---

## 三、医疗健康记录接口

涉及文件：`MedHealthRecordsController.cs`

### 请求：获取所有医疗记录

| 接口说明 | 获取系统中全部医疗健康记录 |
| ----------- | ---------------- |
| HTTP URL | MedHealthRecords |
| HTTP Method | GET |

#### 请求参数

无

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | [\{<br>"recordID": "r1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"recordType": "VACCINATION",<br>"hospitalName": "同济大学附属宠物医院",<br>"diagnosis": "年度猫三联疫苗加强针",<br>"recordDate": "2026-07-15T10:00:00",<br>"nextDueDate": "2027-07-15T10:00:00",<br>"attachmentUrl": null<br>\}] |

### 请求：按猫咪 ID 查询医疗历史

| 接口说明 | 根据猫咪 ID 查询该猫咪的完整医疗健康历史 |
| ----------- | -------------------------- |
| HTTP URL | MedHealthRecords/cat/\{catId\} |
| HTTP Method | GET |

#### 请求参数

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| catId | string | 是 | 猫咪 ID，示例值："c1a2b3d4-e5f6-7890-abcd-ef1234567890" |

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | [\{<br>"recordID": "r1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"recordType": "CHECKUP",<br>"hospitalName": "同济大学附属宠物医院",<br>"diagnosis": "常规体检，各项指标正常",<br>"recordDate": "2026-06-10T09:00:00",<br>"nextDueDate": null,<br>"attachmentUrl": null<br>\},<br>\{<br>"recordID": "r2b3c4d5-e6f7-8901-bcde-f12345678901",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"recordType": "DEWORMING",<br>"hospitalName": "同济大学附属宠物医院",<br>"diagnosis": "体内外驱虫处理",<br>"recordDate": "2026-07-01T11:00:00",<br>"nextDueDate": "2026-10-01T11:00:00",<br>"attachmentUrl": null<br>\}] |

### 请求：根据 ID 获取单条医疗记录

| 接口说明 | 根据记录 ID 获取单条医疗健康记录的详细信息 |
| ----------- | --------------------------- |
| HTTP URL | MedHealthRecords/\{id\} |
| HTTP Method | GET |

#### 请求参数

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| id | string | 是 | 医疗记录 ID，示例值："r1a2b3d4-e5f6-7890-abcd-ef1234567890" |

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 200 | 查询成功 | \{<br>"recordID": "r1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"recordType": "SURGERY",<br>"hospitalName": "同济大学附属宠物医院",<br>"diagnosis": "绝育手术，术后恢复良好",<br>"recordDate": "2026-05-20T14:00:00",<br>"nextDueDate": null,<br>"attachmentUrl": "https://example.com/reports/surgery_20260520.pdf"<br>\} |
| 404 | 未找到 | "未找到 ID 为 r1a2b3d4-e5f6-7890-abcd-ef1234567890 的医疗记录。" |

### 请求：新增医疗记录

| 接口说明 | 为猫咪新增一条医疗健康记录，记录就诊、疾病、用药、绝育、疫苗等信息 |
| ----------- | ------------------------------------------- |
| HTTP URL | MedHealthRecords |
| HTTP Method | POST |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| recordID | string | 否 | 记录 ID，后端自动生成 |
| catID | string | 是 | 猫咪 ID，不能为空，必须在 CAT_CATS 表中存在 |
| recordType | string | 否 | 医疗记录类型，必须为 `VACCINATION`、`CHECKUP`、`TREATMENT`、`SURGERY`、`DEWORMING`、`EMERGENCY`、`OTHER` 之一 |
| hospitalName | string | 否 | 就诊医院名称 |
| diagnosis | string | 否 | 诊断说明 / 用药信息 / 处置描述 |
| recordDate | DateTime | 否 | 就诊日期 |
| nextDueDate | DateTime | 否 | 下次复诊 / 下次疫苗日期 |
| attachmentUrl | string | 否 | 附件链接（报告、发票等） |

#### 请求示例

\{
<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",
<br>"recordType": "VACCINATION",
<br>"hospitalName": "同济大学附属宠物医院",
<br>"diagnosis": "猫三联疫苗第一针",
<br>"recordDate": "2026-08-02T10:00:00",
<br>"nextDueDate": "2026-09-02T10:00:00"
<br>\}

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 201 | 创建成功 | \{<br>"recordID": "6f2c7f6a-0f4b-4c2b-9f2d-1c3f8f7d3d61",<br>"catID": "c1a2b3d4-e5f6-7890-abcd-ef1234567890",<br>"recordType": "VACCINATION",<br>"hospitalName": "同济大学附属宠物医院",<br>"diagnosis": "猫三联疫苗第一针",<br>"recordDate": "2026-08-02T10:00:00",<br>"nextDueDate": "2026-09-02T10:00:00",<br>"attachmentUrl": null<br>\} |
| 400 | 参数不合法 | "医疗记录数据为空，无法创建。"<br>"CatID 不能为空。"<br>"猫咪 CatID='xxx' 不存在。"<br>"无效的医疗类型 'xxx'。允许的类型: VACCINATION, CHECKUP, TREATMENT, SURGERY, DEWORMING, EMERGENCY, OTHER" |

### 请求：编辑医疗记录

| 接口说明 | 编辑已有的医疗健康记录信息 |
| ----------- | ---------------- |
| HTTP URL | MedHealthRecords/\{id\} |
| HTTP Method | PUT |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
| --- | --- | --- | --- |
| recordID | string | 是 | 记录 ID，必须与 URL 中的 ID 一致 |
| catID | string | 是 | 猫咪 ID，不能为空，必须在 CAT_CATS 表中存在 |
| recordType | string | 否 | 医疗记录类型，必须为 `VACCINATION`、`CHECKUP`、`TREATMENT`、`SURGERY`、`DEWORMING`、`EMERGENCY`、`OTHER` 之一 |
| hospitalName | string | 否 | 就诊医院名称 |
| diagnosis | string | 否 | 诊断说明 / 用药信息 / 处置描述 |
| recordDate | DateTime | 否 | 就诊日期 |
| nextDueDate | DateTime | 否 | 下次复诊 / 下次疫苗日期 |
| attachmentUrl | string | 否 | 附件链接（报告、发票等） |

#### 响应体

| 状态码 | 描述 | 响应体 |
| --- | --- | --- |
| 204 | 更新成功 | 无 |
| 400 | 参数不合法 | "医疗记录数据为空，无法更新。"<br>"URL 中的 ID 与请求体中的 ID 不匹配。" |
| 400 | 业务校验失败 | 同新增接口的校验错误信息 |
| 404 | 未找到 | "未找到 ID 为 xxx 的医疗记录，无法更新。" |
