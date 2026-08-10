# C 组（功能点 11-16）：接口核验结果

> 对应接口文档：[C组_救助TNR与医疗接口文档.md]
> 对应测试清单：`组内文件/C组功能点接口Postman测试清单.md`
> 过程文件：`docs/C组核验结果过程文件/`
> 核验模板：[C组_接口核验结果提交模板.md]

## 1. 核验基本信息

| 项目 | 填写内容 |
|---|---|
| 测试人 | 李灿文 |
| 测试日期 | 2026-08-08/09 |
| 代码版本/Commit | 当前 main 分支（截至 2026-08-07） |
| 接口文档版本 | `docs/C组_救助TNR与医疗接口文档.md`|
| 后端地址 | `http://localhost:5047` |
| Oracle 环境 | Oracle 21c，PDB: MYDBPDB1，用户: CAT_SYSTEM |
| 测试账号/角色 | `a_group_admin`（ADMIN），`a_group_volunteer`（VOLUNTEER），`a_group_user`（USER） |
| 是否使用演示数据 | 是（`database/setup_all.sql` 初始化） |

## 2. 接口核验结果

状态说明：`通过`、`不通过`、`未测`、`阻塞`。本次共核验 28 个接口，**19 通过、9 不通过**，发现 16 个问题。

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| C-01 | 查询 TNR 案例列表 | GET | `/api/TnrCases` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-02 | 查询 TNR 案例详情 | GET | `/api/TnrCases/{id}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-03 | 创建 TNR 案例 | POST | `/api/TnrCases` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-04 | 更新 TNR 案例 | PUT | `/api/TnrCases/{id}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-05 | 更新 TNR 状态 | PUT | `/api/TnrCases/{id}/status` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C001、BUG-C011、BUG-C012 |
| C-06 | 查询 TNR 状态日志 | GET | `/api/TnrStatusLogs/case/{caseId}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-07 | 查询医疗记录列表 | GET | `/api/MedHealthRecords` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-08 | 按猫查询医疗记录 | GET | `/api/MedHealthRecords/cat/{catId}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C002 |
| C-09 | 查询医疗记录详情 | GET | `/api/MedHealthRecords/{id}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-10 | 新增医疗记录 | POST | `/api/MedHealthRecords` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-11 | 编辑医疗记录 | PUT | `/api/MedHealthRecords/{id}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-12 | 查询待处理提醒 | GET | `/api/MedReminder` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-13 | 按猫查询提醒 | GET | `/api/MedReminder/cat/{catId}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C003 |
| C-14 | 新增医疗提醒 | POST | `/api/MedReminder` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C004、BUG-C013 |
| C-15 | 查询提醒详情 | GET | `/api/MedReminder/{reminderId}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-16 | 标记提醒已发送 | PUT | `/api/MedReminder/{reminderId}/sent` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-17 | 标记提醒已完成 | PUT | `/api/MedReminder/{reminderId}/complete` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-18 | 查询紧急上报列表 | GET | `/api/EmergencyReports` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-19 | 查询紧急上报详情 | GET | `/api/EmergencyReports/{reportId}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-20 | 提交紧急上报 | POST | `/api/EmergencyReports` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C005 |
| C-21 | 分配紧急上报处理人 | PUT | `/api/EmergencyReports/{reportId}/assign` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-22 | 更新紧急上报状态 | PUT | `/api/EmergencyReports/{reportId}/status` | ADMIN/VOLUNTEER/VET/普通用户/未登录/处理人/非处理人 | 通过 |  |
| C-23 | 查询失踪预警列表 | GET | `/api/MissingAlerts` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-24 | 按猫查询失踪预警 | GET | `/api/MissingAlerts/cat/{catId}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C010 |
| C-25 | 查询失踪预警详情 | GET | `/api/MissingAlerts/{alertId}` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 通过 |  |
| C-26 | 创建目击记录 | POST | `/api/MissingAlerts/sightings` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C006、BUG-C007 |
| C-27 | 创建失踪预警 | POST | `/api/MissingAlerts` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C008、BUG-C009、BUG-C016 |
| C-28 | 更新失踪预警状态 | PUT | `/api/MissingAlerts/{alertId}/status` | ADMIN/VOLUNTEER/VET/普通用户/未登录 | 不通过 | BUG-C014、BUG-C015 |

## 3. 问题详情

### 3.1 问题汇总表

| 问题编号 | 接口编号 | 严重性 | 问题摘要 | 当前状态 |
|---|---|---|---|---|
| BUG-C001 | C-05 | 严重 | TNR 状态可逆向回退（如 RELEASED→DISCOVERED），无业务流程校验 | 待修复 |
| BUG-C002 | C-08 | 一般 | 按猫查询医疗记录，catID 不存在时返回 200 空数组，未返回 404 | 待修复 |
| BUG-C003 | C-13 | 一般 | 按猫查询医疗提醒，catID 不存在时返回 200 空数组，未返回 404 | 待修复 |
| BUG-C004 | C-14 | 严重 | 新增医疗提醒时 catID 不存在，未校验，抛出 Oracle 外键异常 500 | 待修复 |
| BUG-C005 | C-20 | 严重 | 提交紧急上报时 areaID 非法，未校验，抛出 Oracle 外键异常 500 | 待修复 |
| BUG-C006 | C-26 | 严重 | 创建目击记录时 catID 不存在，未校验，抛出 Oracle 外键异常 500 | 待修复 |
| BUG-C007 | C-26 | 严重 | 创建目击记录时 areaID 非法，未校验，抛出 Oracle 外键异常 500 | 待修复 |
| BUG-C008 | C-27 | 严重 | 创建失踪预警时 catID 不存在，未校验，抛出 Oracle 包异常 500 | 待修复 |
| BUG-C009 | C-27 | 严重 | 创建失踪预警时 lastSightingID 不存在，未校验，抛出 Oracle 包异常 500 | 待修复 |
| BUG-C010 | C-24 | 一般 | 按猫查询失踪预警，catID 不存在时返回 200 空数组，未返回 404 | 待修复 |
| BUG-C011 | C-05 | 一般 | 更新 TNR 状态时请求体 operatorID 传入不存在用户，未校验且返回 200 | 待修复 |
| BUG-C012 | C-05 | 一般 | TNR 状态日志 operatorID 始终为登录用户，请求体 operatorID 未被采用 | 待修复 |
| BUG-C013 | C-14 | 严重 | 新增医疗提醒时 receiverUserID 不存在，未校验，抛出 Oracle 外键异常 500 | 待修复 |
| BUG-C014 | C-28 | 一般 | 更新失踪预警状态时请求体 handlerUserID 传入不存在用户，未校验且返回 204 | 待修复 |
| BUG-C015 | C-28 | 一般 | 失踪预警 handlerUserID 实际写入登录用户，请求体值被忽略 | 待修复 |
| BUG-C016 | C-27 | 严重 | 重复创建处理中预警，未转换为 409，抛出 Oracle 业务异常 500 | 待修复 |

### 3.2 问题详情文本块

```text
问题编号：BUG-C001
接口编号：C-05
测试人/时间：李灿文/2026-08-08
严重性：严重
前置条件：已存在 currentStatus=RELEASED 的 TNR 案例（caseID=e5aa77bb-2bd6-4d51-ba94-e49fb264795a）
请求方法与 URL：PUT /api/TnrCases/{caseId}/status
请求头（隐藏 Token）：Content-Type: application/json
请求参数/请求体：{"newStatus": "DISCOVERED", "operatorID": "user-admin-a-group", "remark": "..."}
预期结果：状态由 RELEASED 回退到 DISCOVERED 应被拒绝（业务流程不可逆），返回 400
实际结果：返回 200，状态更新成功并生成流转日志
响应状态码与响应体：200 {"oldStatus":"RELEASED","newStatus":"DISCOVERED","message":"状态更新成功，已生成流转日志。"}
复现步骤：
1. 创建 TNR 案例并走完 DISCOVERED→CAPTURED→SURGERY→RECOVERING→RELEASED 流程
2. 调用 C-05 将 newStatus 设为 DISCOVERED
3. 观察返回 200 且日志记录 RELEASED→DISCOVERED
附件/日志：C05-BUG-C001-状态转换不合理但合法.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C002
接口编号：C-08
测试人/时间：李灿文/2026-08-08
严重性：一般
前置条件：catID=demo-cat-campus-999 不存在
请求方法与 URL：GET /api/MedHealthRecords/cat/demo-cat-campus-999
请求头：Authorization: Bearer {{adminToken}}
预期结果：按接口文档应返回 404（记录或猫咪不存在）
实际结果：返回 200 与空数组 []
响应状态码与响应体：200 "[]"
复现步骤：
1. 使用任意不存在的 catID 查询
2. 观察返回 200 + 空数组
附件/日志：C08-BUG-C002-按猫咪查询医疗记录，未检查ID是否存在.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C003
接口编号：C-13
测试人/时间：李灿文/2026-08-09
严重性：一般
前置条件：catID=demo-cat-campus-999 不存在
请求方法与 URL：GET /api/MedReminder/cat/demo-cat-campus-999
预期结果：返回 404
实际结果：返回 200 与空数组 []
响应状态码与响应体：200 "[]"
复现步骤：同 BUG-C002，路径为 /api/MedReminder/cat/{catId}
附件/日志：C13-BUG-C003-按猫咪查询医疗提醒，未检查ID是否存在.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C004
接口编号：C-14
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：catID=demo-cat-campus-999 不存在
请求方法与 URL：POST /api/MedReminder
请求体：{"catID":"demo-cat-campus-999","reminderType":"VACCINATION","receiverUserID":"user-volunteer-a-group","reminderTime":"2026-08-24T09:00:00Z"}
预期结果：返回 400 或 404 提示猫咪不存在
实际结果：未做外键存在性校验，直接调用存储过程，Oracle 抛出外键约束异常并泄漏完整堆栈
响应状态码与响应体：500 ORA-02291: 违反完整约束条件 (CAT_SYSTEM.SYS_C008264) - 未找到父项关键字（堆栈至 MedReminderRepository.CreateReminder）
复现步骤：
1. POST /api/MedReminder 传入不存在的 catID
2. 观察 500 与 Oracle 异常堆栈
附件/日志：C14-BUG-C004-猫咪ID不存在，发生报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C005
接口编号：C-20
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：areaID="000" 不存在
请求方法与 URL：POST /api/EmergencyReports
请求体：{"areaID":"000","animalType":"CAT","urgencyLevel":"HIGH",...}
预期结果：按接口文档返回 404（区域不存在）
实际结果：未校验 areaID 存在性，Oracle 抛出外键约束异常 500
响应状态码与响应体：500 ORA-02291: 违反完整约束条件 (CAT_SYSTEM.SYS_C008329)（堆栈至 EmergencyReportRepository.Create, PKG_RESCUE_CARE line 120）
复现步骤：POST /api/EmergencyReports 传入不存在的 areaID
附件/日志：C20-BUG-C005-地区ID非法，报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C006
接口编号：C-26
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：catID=demo-cat-campus-999 不存在
请求方法与 URL：POST /api/MissingAlerts/sightings
请求体：{"catID":"demo-cat-campus-999","areaID":"demo-area-library","sightingTime":"..."}
预期结果：返回 400 或 404 提示猫咪不存在
实际结果：未校验，Oracle 抛出外键约束异常 500
响应状态码与响应体：500 ORA-02291 (CAT_SYSTEM.SYS_C008247)（堆栈至 MissingAlertRepository.CreateSighting, PKG_RESCUE_CARE line 204）
附件/日志：C26-BUG-C006-猫咪ID不存在，报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C007
接口编号：C-26
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：areaID="000" 不存在
请求方法与 URL：POST /api/MissingAlerts/sightings
请求体：{"catID":"demo-cat-campus-001","areaID":"000","sightingTime":"..."}
预期结果：返回 400 或 404 提示区域不存在
实际结果：未校验，Oracle 抛出外键约束异常 500
响应状态码与响应体：500 ORA-02291 (CAT_SYSTEM.SYS_C008249)
附件/日志：C26-BUG-C007-地区ID非法，报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C008
接口编号：C-27
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：catID=demo-cat-campus-999 不存在，lastSightingID 属于其他猫
请求方法与 URL：POST /api/MissingAlerts
请求体：{"catID":"demo-cat-campus-999","lastSightingID":"f1dcb713...","thresholdDays":7,...}
预期结果：返回 400 或 404 提示猫咪不存在
实际结果：未校验 catID，存储过程抛出业务异常并泄漏堆栈 500
响应状态码与响应体：500 ORA-20162: Last sighting does not belong to the specified cat（PKG_RESCUE_CARE line 246）
附件/日志：C27-BUG-C008-猫咪ID不存在，报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C009
接口编号：C-27
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：lastSightingID="123456" 不存在
请求方法与 URL：POST /api/MissingAlerts
请求体：{"catID":"demo-cat-campus-001","lastSightingID":"123456","thresholdDays":7,...}
预期结果：返回 404 提示最后目击不存在（接口文档要求）
实际结果：未校验，存储过程抛出异常并泄漏堆栈 500
响应状态码与响应体：500 ORA-20162: Last sighting does not belong to the specified cat
附件/日志：C27-BUG-C009-目击记录ID不存在，报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C010
接口编号：C-24
测试人/时间：李灿文/2026-08-09
严重性：一般
前置条件：catID=demo-cat-campus-999 不存在
请求方法与 URL：GET /api/MissingAlerts/cat/demo-cat-campus-999
预期结果：返回 404
实际结果：返回 200 与空数组 []
响应状态码与响应体：200 "[]"
附件/日志：C24-BUG-C010-按猫咪查询失踪预警，未检查ID是否存在.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C011
接口编号：C-05
测试人/时间：李灿文/2026-08-09
严重性：一般
前置条件：已存在 TNR 案例；operatorID="特朗普" 不存在
请求方法与 URL：PUT /api/TnrCases/{caseId}/status
请求体：{"newStatus":"CAPTURED","operatorID":"特朗普","remark":"..."}
预期结果：对请求体传入的不存在 operatorID 应校验并返回 400，或文档明确该字段被忽略
实际结果：返回 200 成功（实际用登录用户覆盖 operatorID，未对非法输入报错）
响应状态码与响应体：200 {"oldStatus":"CAPTURED","newStatus":"CAPTURED","message":"状态更新成功，已生成流转日志。"}
附件/日志：C05-BUG-C011-未检查操作者ID是否存在.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C012
接口编号：C-05（通过 C-06 查询日志验证）
测试人/时间：李灿文/2026-08-09
严重性：一般
前置条件：在 BUG-C011 操作后查询状态日志
请求方法与 URL：GET /api/TnrStatusLogs/case/{caseId}
预期结果：若接口接受 operatorID，日志应记录请求体传入的操作人；若不接受，文档应删除该字段
实际结果：日志 operatorID 始终为登录用户 user-admin-a-group，请求体 operatorID="特朗普" 未被采用
响应状态码与响应体：200，5 条日志 operatorID 均为 user-admin-a-group
附件/日志：C05-BUG-C012-状态转换后未更新操作者（特朗普）.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C013
接口编号：C-14
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：receiverUserID="特朗普" 不存在
请求方法与 URL：POST /api/MedReminder
请求体：{"catID":"demo-cat-campus-001","reminderType":"VACCINATION","receiverUserID":"特朗普","reminderTime":"..."}
预期结果：返回 400 或 404 提示接收者不存在
实际结果：未校验 receiverUserID，Oracle 抛出外键约束异常 500
响应状态码与响应体：500 ORA-02291 (CAT_SYSTEM.SYS_C008265)
附件/日志：C14-BUG-C013-接收者ID不存在，报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C014
接口编号：C-28
测试人/时间：李灿文/2026-08-09
严重性：一般
前置条件：已存在失踪预警；handlerUserID="特朗普" 不存在
请求方法与 URL：PUT /api/MissingAlerts/{alertId}/status
请求体：{"alertStatus":"FOUND","handlerUserID":"特朗普","remark":"..."}
预期结果：对请求体传入的不存在 handlerUserID 应校验并返回 400
实际结果：返回 204 成功（实际用登录用户覆盖 handlerUserID，未对非法输入报错）
响应状态码与响应体：204 No Content
附件/日志：C28-BUG-C014-未检查用户ID是否存在.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C015
接口编号：C-28（通过 C-25 查询详情验证）
测试人/时间：李灿文/2026-08-09
严重性：一般
前置条件：在 BUG-C014 操作后查询预警详情
请求方法与 URL：GET /api/MissingAlerts/{alertId}
预期结果：若接口接受 handlerUserID，应写入请求体值；若不接受，文档应删除该字段或标注由服务端填充
实际结果：handlerUserID 实际写入登录用户 user-admin-a-group，而非请求体传入的"特朗普"；remark 中保留了"特朗普"字样
响应状态码与响应体：200 {"alertID":"...","alertStatus":"FOUND","handlerUserID":"user-admin-a-group","remark":"特朗普已在南校区找到图图，状态良好"}
附件/日志：C28-BUG-C015-未更新处理者ID（特朗普）.example.yaml
建议处理人：C 组
复测结果：未修复
```

```text
问题编号：BUG-C016
接口编号：C-27
测试人/时间：李灿文/2026-08-09
严重性：严重
前置条件：catID=demo-cat-campus-001 已存在 PROCESSING 状态的失踪预警
请求方法与 URL：POST /api/MissingAlerts
请求体：{"catID":"demo-cat-campus-001","lastSightingID":"f1dcb713b60b4ccdaade5c671dce27d8","lastSightingTime":"2026-08-06T18:00:00Z","thresholdDays":7,"remark":"..."}
预期结果：按接口文档应返回 409（已存在处理中预警）
实际结果：未在 Controller 层捕获业务冲突，存储过程抛出 ORA-20163 异常并泄漏完整堆栈，返回 500
响应状态码与响应体：500 ORA-20163: The cat already has an active missing alert（PKG_RESCUE_CARE line 254）
复现步骤：
1. 为某猫创建一条失踪预警（PROCESSING）
2. 再次以同一 catID 调用 POST /api/MissingAlerts
3. 观察 500 与 Oracle 异常堆栈
附件/日志：C27-BUG-C016-已存在处理中的失踪预警，再次创建报错.example.yaml
建议处理人：C 组
复测结果：未修复
```

## 4. 提交前检查

- [√] 已填写测试人、日期、代码版本和数据库环境。
- [√] 28 个接口均已填写状态。
- [√] 每个不通过项都有问题编号和复现信息。
- [√] 已隐藏 Token、密码、数据库账号和服务器凭据（过程文件中 500 响应体含 JWT，建议清理后再对外提交）。

## 5. 补充测试

以下测试项均已全部完成，未发现新增 BUG：

- **权限边界测试**：对所有 28 个接口补充了多类身份的访问测试，共 97 个用例，全部符合预期：
  - C-01~C-17（需 ADMIN/VOLUNTEER/VET）：志愿者/兽医访问返回 `200/201/204`，普通用户 `403`，未登录 `401`。
  - C-18~C-20、C-23~C-27（仅需登录）：志愿者/兽医/普通用户访问返回 `200/201/204`，未登录 `401`。
  - C-21、C-22、C-28（需 ADMIN/VOLUNTEER）：志愿者访问 `204`，兽医/普通用户 `403`，未登录 `401`。
- **C-22 处理人/非处理人业务校验**：C-22 更新紧急上报处理状态时，除角色校验外还有处理人校验——被分配的处理人本人访问返回 `204`，非处理人（其他VOLUNTEER）访问返回 `403`，业务权限控制正确。

---
