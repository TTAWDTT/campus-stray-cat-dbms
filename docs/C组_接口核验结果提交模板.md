# C 组（功能点 11-16）：接口核验结果提交模板

对应文档：[C组_救助TNR与医疗接口文档.md](C组_救助TNR与医疗接口文档.md)

## 1. 核验基本信息

| 项目 | 填写内容 |
|---|---|
| 测试人 |  |
| 测试日期 |  |
| 代码版本/Commit |  |
| 接口文档版本 |  |
| 后端地址 | `http://localhost:5047` |
| Oracle 环境 |  |
| 测试账号/角色 |  |
| 是否使用演示数据 | 是 / 否 |

## 2. 接口核验结果

状态统一填写：`未测`、`通过`、`不通过`、`阻塞`。不通过或阻塞项必须填写问题详情。

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| C-01 | 查询 TNR 案例列表 | GET | `/api/TnrCases` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-02 | 查询 TNR 案例详情 | GET | `/api/TnrCases/{id}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-03 | 创建 TNR 案例 | POST | `/api/TnrCases` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-04 | 更新 TNR 案例 | PUT | `/api/TnrCases/{id}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-05 | 更新 TNR 状态 | PUT | `/api/TnrCases/{id}/status` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-06 | 查询 TNR 状态日志 | GET | `/api/TnrStatusLogs/case/{caseId}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-07 | 查询医疗记录列表 | GET | `/api/MedHealthRecords` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-08 | 按猫查询医疗记录 | GET | `/api/MedHealthRecords/cat/{catId}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-09 | 查询医疗记录详情 | GET | `/api/MedHealthRecords/{id}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-10 | 新增医疗记录 | POST | `/api/MedHealthRecords` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-11 | 编辑医疗记录 | PUT | `/api/MedHealthRecords/{id}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-12 | 查询待处理提醒 | GET | `/api/MedReminder` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-13 | 按猫查询提醒 | GET | `/api/MedReminder/cat/{catId}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-14 | 新增医疗提醒 | POST | `/api/MedReminder` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-15 | 查询提醒详情 | GET | `/api/MedReminder/{reminderId}` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-16 | 标记提醒已发送 | PUT | `/api/MedReminder/{reminderId}/sent` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-17 | 标记提醒已完成 | PUT | `/api/MedReminder/{reminderId}/complete` | ADMIN/VOLUNTEER/VET | 未测 |  |
| C-18 | 查询紧急上报列表 | GET | `/api/EmergencyReports` | 已登录 | 未测 |  |
| C-19 | 查询紧急上报详情 | GET | `/api/EmergencyReports/{reportId}` | 已登录 | 未测 |  |
| C-20 | 提交紧急上报 | POST | `/api/EmergencyReports` | 已登录 | 未测 |  |
| C-21 | 分配紧急上报处理人 | PUT | `/api/EmergencyReports/{reportId}/assign` | ADMIN/VOLUNTEER | 未测 |  |
| C-22 | 更新紧急上报状态 | PUT | `/api/EmergencyReports/{reportId}/status` | ADMIN/当前处理人 | 未测 |  |
| C-23 | 查询失踪预警列表 | GET | `/api/MissingAlerts` | 已登录 | 未测 |  |
| C-24 | 按猫查询失踪预警 | GET | `/api/MissingAlerts/cat/{catId}` | 已登录 | 未测 |  |
| C-25 | 查询失踪预警详情 | GET | `/api/MissingAlerts/{alertId}` | 已登录 | 未测 |  |
| C-26 | 创建目击记录 | POST | `/api/MissingAlerts/sightings` | 已登录 | 未测 |  |
| C-27 | 创建失踪预警 | POST | `/api/MissingAlerts` | 已登录 | 未测 |  |
| C-28 | 更新失踪预警状态 | PUT | `/api/MissingAlerts/{alertId}/status` | ADMIN/VOLUNTEER | 未测 |  |

## 3. 问题详情

| 问题编号 | 接口编号 | 严重性 | 测试数据/前置条件 | 预期结果 | 实际结果 | 响应码/错误信息 | 附件/链接 | 当前状态 |
|---|---|---|---|---|---|---|---|---|
| BUG-C-001 |  | 阻塞/严重/一般 |  |  |  |  |  | 待修复 |

```text
问题编号：BUG-C-___
接口编号：___
测试人/时间：___
严重性：阻塞 / 严重 / 一般
前置条件：
请求方法与 URL：
请求头（隐藏 Token）：
请求参数/请求体：
预期结果：
实际结果：
响应状态码与响应体：
复现步骤：
1.
2.
附件/日志：
建议处理人：
复测结果：未修复 / 已修复 / 无法复测
```

## 4. 提交前检查

- [ ] 已填写测试人、日期、代码版本和数据库环境。
- [ ] 28 个接口均已填写状态。
- [ ] 每个不通过或阻塞项都有问题编号和复现信息。
- [ ] 已隐藏 Token、密码、数据库账号和服务器凭据。
