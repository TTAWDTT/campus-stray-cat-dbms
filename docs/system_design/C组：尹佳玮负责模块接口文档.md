## 一、医疗提醒接口

涉及文件：`MedReminderController.cs`

### 请求：获取待处理提醒列表

| 接口说明        | 获取待处理或已发送的医疗提醒列表                      |
| ----------- | ------------------------------------- |
| HTTP URL    | MedReminder |
| HTTP Method | GET                                   |

#### 请求参数

无

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|[\{<br>"reminderID": "reminder-001",<br>"recordID": "record-001",<br>"catID": "cat-001",<br>"reminderType": "VACCINATION",<br>"receiverUserID": "user-002",<br>"reminderTime": "2026-08-30T09:00:00",<br>"sendStatus": "PENDING"<br>\}]|

### 请求：按猫咪 ID 查询提醒历史

| 接口说明        | 按猫咪 ID 查询该猫咪的医疗提醒历史                                 |
| ----------- | --------------------------------------------------- |
| HTTP URL    | MedReminder/cat/\{catId\} |
| HTTP Method | GET                                                 |

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|catId|string|是|猫咪 ID，示例值："cat-001"|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|[\{<br>"reminderID": "reminder-001",<br>"recordID": "record-001",<br>"catID": "cat-001",<br>"reminderType": "VACCINATION",<br>"receiverUserID": "user-002",<br>"reminderTime": "2026-08-30T09:00:00",<br>"sendStatus": "PENDING"<br>\}]|
|400|参数不合法|"猫咪 ID 不能为空。"|

### 请求：新增医疗提醒

|接口说明|新增一条疫苗、驱虫、手术、复查等医疗提醒|
|---|---|
|HTTP URL|MedReminder|
|HTTP Method|POST|

#### 请求体

|名称|类型|必填|描述|
|---|---|---|---|
|reminderID|string|否|提醒 ID，由数据库程序包自动生成|
|recordID|string|否|关联的医疗记录 ID，可以为空|
|catID|string|是|猫咪 ID，不能为空|
|reminderType|string|是|提醒类型，必须为 `VACCINATION`、`CHECKUP`、`TREATMENT`、`SURGERY`、`DEWORMING`、`EMERGENCY`、`OTHER` 之一|
|receiverUserID|string|否|接收提醒的用户 ID|
|reminderTime|DateTime|是|提醒时间，不能为空|
|sendStatus|string|否|发送状态，创建时由数据库程序包设置为 `PENDING`|

#### 请求示例

\{
<br>"recordID": "record-001",
<br>"catID": "cat-001",
<br>"reminderType": "VACCINATION",
<br>"receiverUserID": "user-002",
<br>"reminderTime": "2026-08-30T09:00:00"
<br>\}

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|201|创建成功|\{<br>"reminderID": "generated-reminder-id",<br>"recordID": "record-001",<br>"catID": "cat-001",<br>"reminderType": "VACCINATION",<br>"receiverUserID": "user-002",<br>"reminderTime": "2026-08-30T09:00:00",<br>"sendStatus": "PENDING"<br>\}|
|400|参数不合法|"提醒数据不能为空。"<br>"猫咪 ID 不能为空。"<br>"提醒类型必须是 VACCINATION、CHECKUP、TREATMENT、SURGERY、DEWORMING、EMERGENCY、OTHER。"<br>"提醒时间不能为空。"|

### 请求：根据 ID 查看提醒详情

|接口说明|根据提醒 ID 查询单条医疗提醒|
|---|---|
|HTTP URL|MedReminder/\{reminderId\}|
|HTTP Method|GET|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|reminderId|string|是|提醒 ID，示例值："reminder-001"|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|\{<br>"reminderID": "reminder-001",<br>"recordID": "record-001",<br>"catID": "cat-001",<br>"reminderType": "VACCINATION",<br>"receiverUserID": "user-002",<br>"reminderTime": "2026-08-30T09:00:00",<br>"sendStatus": "PENDING"<br>\}|
|400|参数不合法|"提醒 ID 不能为空。"|
|404|未找到|"未找到提醒 reminder-001。"|

### 请求：标记提醒为已发送

|接口说明|将医疗提醒状态更新为 `SENT`|
|---|---|
|HTTP URL|MedReminder/\{reminderId\}/sent|
|HTTP Method|PUT|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|reminderId|string|是|提醒 ID，示例值："reminder-001"|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|204|更新成功|无|
|400|参数不合法|"提醒 ID 不能为空。"|
|404|未找到|"未找到提醒 reminder-001。"|

### 请求：标记提醒为已完成

|接口说明|将医疗提醒状态更新为 `COMPLETED`|
|---|---|
|HTTP URL|MedReminder/\{reminderId\}/complete|
|HTTP Method|PUT|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|reminderId|string|是|提醒 ID，示例值："reminder-001"|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|204|更新成功|无|
|400|参数不合法|"提醒 ID 不能为空。"|
|404|未找到|"未找到提醒 reminder-001。"|

## 二、紧急救助上报接口

涉及文件：`EmergencyReportsController.cs`

### 请求：获取全部紧急救助上报

|接口说明|获取全部紧急救助上报记录|
|---|---|
|HTTP URL|EmergencyReports|
|HTTP Method|GET|

#### 请求参数

无

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|[\{<br>"reportID": "report-001",<br>"reporterUserID": "user-001",<br>"areaID": "area-001",<br>"animalType": "CAT",<br>"photoURL": "https://example.com/cat.jpg",<br>"longitude": 121.215,<br>"latitude": 31.289,<br>"reportTime": "2026-08-01T10:00:00",<br>"urgencyLevel": "HIGH",<br>"processStatus": "SUBMITTED",<br>"handlerUserID": null,<br>"processResult": null<br>\}]|

### 请求：根据 ID 查询紧急救助上报

|接口说明|根据上报 ID 查询单条紧急救助上报|
|---|---|
|HTTP URL|EmergencyReports/\{reportId\}|
|HTTP Method|GET|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|reportId|string|是|紧急上报 ID，示例值："report-001"|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|\{<br>"reportID": "report-001",<br>"reporterUserID": "user-001",<br>"areaID": "area-001",<br>"animalType": "CAT",<br>"photoURL": "https://example.com/cat.jpg",<br>"longitude": 121.215,<br>"latitude": 31.289,<br>"reportTime": "2026-08-01T10:00:00",<br>"urgencyLevel": "HIGH",<br>"processStatus": "SUBMITTED",<br>"handlerUserID": null,<br>"processResult": null<br>\}|
|400|参数不合法|"上报 ID 不能为空。"|
|404|未找到|"未找到上报 report-001。"|

### 请求：提交紧急救助上报

|接口说明|普通用户提交受伤、被困、疑似生病等紧急救助上报|
|---|---|
|HTTP URL|EmergencyReports|
|HTTP Method|POST|

#### 请求体

|名称|类型|必填|描述|
|---|---|---|---|
|reportID|string|否|上报 ID，由数据库程序包自动生成|
|reporterUserID|string|是|上报人用户 ID，不能为空|
|areaID|string|是|校内区域 ID，不能为空|
|animalType|string|是|动物类型，不能为空，示例值：`CAT`|
|photoURL|string|否|现场图片 URL|
|longitude|decimal|否|经度|
|latitude|decimal|否|纬度|
|reportTime|DateTime|否|上报时间，由数据库程序包自动记录|
|urgencyLevel|string|是|紧急等级，必须为 `LOW`、`MEDIUM`、`HIGH`、`CRITICAL` 之一|
|processStatus|string|否|处理状态，创建时由数据库程序包设置为 `SUBMITTED`|
|handlerUserID|string|否|处理人用户 ID，提交时可为空|
|processResult|string|否|处理结果，提交时可为空|

#### 请求示例

\{
<br>"reporterUserID": "user-001",
<br>"areaID": "area-001",
<br>"animalType": "CAT",
<br>"photoURL": "https://example.com/cat.jpg",
<br>"longitude": 121.215,
<br>"latitude": 31.289,
<br>"urgencyLevel": "HIGH"
<br>\}

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|201|创建成功|\{<br>"reportID": "generated-report-id",<br>"reporterUserID": "user-001",<br>"areaID": "area-001",<br>"animalType": "CAT",<br>"photoURL": "https://example.com/cat.jpg",<br>"longitude": 121.215,<br>"latitude": 31.289,<br>"reportTime": "2026-08-01T10:00:00",<br>"urgencyLevel": "HIGH",<br>"processStatus": "SUBMITTED",<br>"handlerUserID": null,<br>"processResult": null<br>\}|
|400|参数不合法|"上报数据不能为空。"<br>"上报人 ID 不能为空。"<br>"区域 ID 不能为空。"<br>"动物类型不能为空。"<br>"紧急等级必须是 LOW、MEDIUM、HIGH 或 CRITICAL。"|

### 请求：分配紧急上报处理人

|接口说明|管理员或志愿者为紧急救助上报分配处理人|
|---|---|
|HTTP URL|EmergencyReports/\{reportId\}/assign|
|HTTP Method|PUT|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|reportId|string|是|紧急上报 ID，示例值："report-001"|

#### 请求体

|名称|类型|必填|描述|
|---|---|---|---|
|handlerUserId|string|是|处理人用户 ID，不能为空。请求体直接传字符串，例如 `"user-002"`|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|204|分配成功|无|
|400|参数不合法|"上报 ID 不能为空。"<br>"处理人 ID 不能为空。"|
|404|未找到|"未找到上报 report-001。"|

### 请求：更新紧急上报处理状态

|接口说明|更新紧急救助上报的处理状态和处理结果|
|---|---|
|HTTP URL|EmergencyReports/\{reportId\}/status|
|HTTP Method|PUT|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|reportId|string|是|紧急上报 ID，示例值："report-001"|

#### 请求体

|名称|类型|必填|描述|
|---|---|---|---|
|processStatus|string|是|处理状态，必须为 `SUBMITTED`、`ASSIGNED`、`PROCESSING`、`RESOLVED`、`CLOSED` 之一|
|processResult|string|否|处理结果说明|

#### 请求示例

\{
<br>"processStatus": "RESOLVED",
<br>"processResult": "志愿者已到场并送医处理。"
<br>\}

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|204|更新成功|无|
|400|参数不合法|"状态更新数据不能为空。"<br>"上报 ID 不能为空。"<br>"处理状态必须是 SUBMITTED、ASSIGNED、PROCESSING、RESOLVED 或 CLOSED。"|
|404|未找到|"未找到上报 report-001。"|

## 三、猫咪失踪预警接口

涉及文件：`MissingAlertsController.cs`

### 请求：获取全部失踪预警

|接口说明|获取全部猫咪失踪预警记录|
|---|---|
|HTTP URL|MissingAlerts|
|HTTP Method|GET|

#### 请求参数

无

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|[\{<br>"alertID": "alert-001",<br>"catID": "cat-001",<br>"lastSightingID": "sighting-001",<br>"lastSightingTime": "2026-07-25T18:30:00",<br>"thresholdDays": 7,<br>"alertTime": "2026-08-01T10:00:00",<br>"alertStatus": "PROCESSING",<br>"handlerUserID": "user-002",<br>"closeTime": null,<br>"remark": "超过 7 天未目击"<br>\}]|

### 请求：按猫咪 ID 查询失踪预警

|接口说明|按猫咪 ID 查询该猫咪的失踪预警历史|
|---|---|
|HTTP URL|MissingAlerts/cat/\{catId\}|
|HTTP Method|GET|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|catId|string|是|猫咪 ID，示例值："cat-001"|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|[\{<br>"alertID": "alert-001",<br>"catID": "cat-001",<br>"lastSightingID": "sighting-001",<br>"lastSightingTime": "2026-07-25T18:30:00",<br>"thresholdDays": 7,<br>"alertTime": "2026-08-01T10:00:00",<br>"alertStatus": "PROCESSING",<br>"handlerUserID": "user-002",<br>"closeTime": null,<br>"remark": "超过 7 天未目击"<br>\}]|
|400|参数不合法|"猫咪 ID 不能为空。"|

### 请求：根据 ID 查询失踪预警

|接口说明|根据预警 ID 查询单条失踪预警|
|---|---|
|HTTP URL|MissingAlerts/\{alertId\}|
|HTTP Method|GET|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|alertId|string|是|预警 ID，示例值："alert-001"|

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|查询成功|\{<br>"alertID": "alert-001",<br>"catID": "cat-001",<br>"lastSightingID": "sighting-001",<br>"lastSightingTime": "2026-07-25T18:30:00",<br>"thresholdDays": 7,<br>"alertTime": "2026-08-01T10:00:00",<br>"alertStatus": "PROCESSING",<br>"handlerUserID": "user-002",<br>"closeTime": null,<br>"remark": "超过 7 天未目击"<br>\}|
|400|参数不合法|"预警 ID 不能为空。"|
|404|未找到|"未找到预警 alert-001。"|

### 请求：创建猫咪目击记录

|接口说明|记录一次猫咪目击信息，用于失踪预警中的最后目击信息|
|---|---|
|HTTP URL|MissingAlerts/sightings|
|HTTP Method|POST|

#### 请求体

|名称|类型|必填|描述|
|---|---|---|---|
|sightingID|string|否|目击记录 ID，由数据库程序包自动生成|
|catID|string|是|猫咪 ID，不能为空|
|userID|string|否|上报目击信息的用户 ID|
|areaID|string|是|目击区域 ID，不能为空|
|longitude|decimal|否|目击位置经度|
|latitude|decimal|否|目击位置纬度|
|photoURL|string|否|目击照片 URL|
|sightingTime|DateTime|是|目击时间，不能为空|
|remark|string|否|备注说明|

#### 请求示例

\{
<br>"catID": "cat-001",
<br>"userID": "user-001",
<br>"areaID": "area-001",
<br>"longitude": 121.215,
<br>"latitude": 31.289,
<br>"photoURL": "https://example.com/sighting-cat.jpg",
<br>"sightingTime": "2026-07-25T18:30:00",
<br>"remark": "最后一次在图书馆东门附近看到"
<br>\}

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|200|创建成功|\{<br>"sightingID": "generated-sighting-id",<br>"catID": "cat-001",<br>"userID": "user-001",<br>"areaID": "area-001",<br>"longitude": 121.215,<br>"latitude": 31.289,<br>"photoURL": "https://example.com/sighting-cat.jpg",<br>"sightingTime": "2026-07-25T18:30:00",<br>"remark": "最后一次在图书馆东门附近看到"<br>\}|
|400|参数不合法|"目击记录不能为空。"<br>"猫咪 ID 不能为空。"<br>"区域 ID 不能为空。"<br>"目击时间不能为空。"|

### 请求：创建失踪预警

|接口说明|创建猫咪失踪预警，记录最后目击、阈值天数和处理信息|
|---|---|
|HTTP URL|MissingAlerts|
|HTTP Method|POST|

#### 请求体

|名称|类型|必填|描述|
|---|---|---|---|
|alertID|string|否|预警 ID，由数据库程序包自动生成|
|catID|string|是|猫咪 ID，不能为空|
|lastSightingID|string|否|最后目击记录 ID|
|lastSightingTime|DateTime|否|最后目击时间|
|thresholdDays|int|否|失踪判断阈值天数；如果填写，必须大于 0|
|alertTime|DateTime|否|预警发布时间，由数据库程序包自动记录|
|alertStatus|string|否|预警状态，创建时由数据库程序包设置为 `PROCESSING`|
|handlerUserID|string|否|处理人用户 ID|
|closeTime|DateTime|否|关闭时间，创建时为空|
|remark|string|否|备注说明|

#### 请求示例

\{
<br>"catID": "cat-001",
<br>"lastSightingID": "sighting-001",
<br>"lastSightingTime": "2026-07-25T18:30:00",
<br>"thresholdDays": 7,
<br>"handlerUserID": "user-002",
<br>"remark": "超过 7 天未目击，发布寻猫预警"
<br>\}

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|201|创建成功|\{<br>"alertID": "generated-alert-id",<br>"catID": "cat-001",<br>"lastSightingID": "sighting-001",<br>"lastSightingTime": "2026-07-25T18:30:00",<br>"thresholdDays": 7,<br>"alertTime": "2026-08-01T10:00:00",<br>"alertStatus": "PROCESSING",<br>"handlerUserID": "user-002",<br>"closeTime": null,<br>"remark": "超过 7 天未目击，发布寻猫预警"<br>\}|
|400|参数不合法|"预警数据不能为空。"<br>"猫咪 ID 不能为空。"<br>"阈值天数必须大于 0。"|

### 请求：更新失踪预警状态

|接口说明|更新失踪预警状态为处理中、已寻回或关闭|
|---|---|
|HTTP URL|MissingAlerts/\{alertId\}/status|
|HTTP Method|PUT|

#### 请求参数

|名称|类型|必填|描述|
|---|---|---|---|
|alertId|string|是|预警 ID，示例值："alert-001"|

#### 请求体

|名称|类型|必填|描述|
|---|---|---|---|
|alertStatus|string|是|预警状态，必须为 `PROCESSING`、`FOUND`、`CLOSED` 之一|
|handlerUserID|string|是|处理人用户 ID，不能为空|
|remark|string|否|处理备注|

#### 请求示例

\{
<br>"alertStatus": "FOUND",
<br>"handlerUserID": "user-002",
<br>"remark": "已在图书馆东门附近寻回"
<br>\}

#### 响应体

|状态码|描述|响应体|
|---|---|---|
|204|更新成功|无|
|400|参数不合法|"状态更新数据不能为空。"<br>"预警 ID 不能为空。"<br>"预警状态必须是 PROCESSING、FOUND 或 CLOSED。"<br>"处理人 ID 不能为空。"|
|404|未找到|"未找到预警 alert-001。"|
