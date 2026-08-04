# D 组（功能点 17-21）：接口核验结果提交模板

对应文档：[D组_领养志愿者投喂财务接口文档.md](D组_领养志愿者投喂财务接口文档.md)

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
| D-01 | 查询待审核领养申请 | GET | `/api/adoption-workflow/pending` | ADMIN/VOLUNTEER | 未测 |  |
| D-02 | 查询领养回访汇总 | GET | `/api/adoption-workflow/visits` | ADMIN/VOLUNTEER | 未测 |  |
| D-03 | 提交领养申请 | POST | `/api/adoption-workflow/applications` | 已登录 | 未测 |  |
| D-04 | 审核领养申请 | POST | `/api/adoption-workflow/applications/{applicationId}/review` | ADMIN/VOLUNTEER | 未测 |  |
| D-05 | 新增领养回访 | POST | `/api/adoption-workflow/applications/{applicationId}/visits` | ADMIN/VOLUNTEER | 未测 |  |
| D-06 | 查询志愿者看板 | GET | `/api/volunteer-workflow/activity` | ADMIN/VOLUNTEER | 未测 |  |
| D-07 | 注册志愿者 | POST | `/api/volunteer-workflow/volunteers` | ADMIN | 未测 |  |
| D-08 | 新建志愿者排班 | POST | `/api/volunteer-workflow/shifts` | ADMIN | 未测 |  |
| D-09 | 排班签到 | POST | `/api/volunteer-workflow/shifts/{shiftId}/checkins` | VOLUNTEER | 未测 |  |
| D-10 | 新增积分日志 | POST | `/api/volunteer-workflow/credit-logs` | ADMIN | 未测 |  |
| D-11 | 查询全部投喂任务 | GET | `/api/feeding-tasks` | ADMIN/VOLUNTEER | 未测 |  |
| D-12 | 查询投喂任务详情 | GET | `/api/feeding-tasks/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| D-13 | 按志愿者查询任务 | GET | `/api/feeding-tasks/by-volunteer/{volunteerId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-14 | 按点位查询任务 | GET | `/api/feeding-tasks/by-point/{pointId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-15 | 按状态查询任务 | GET | `/api/feeding-tasks/by-status/{status}` | ADMIN/VOLUNTEER | 未测 |  |
| D-16 | 新增投喂任务 | POST | `/api/feeding-tasks` | ADMIN/VOLUNTEER | 未测 |  |
| D-17 | 更新投喂任务 | PUT | `/api/feeding-tasks/{id}` | ADMIN/任务负责人 | 未测 |  |
| D-18 | 更新投喂任务状态 | PUT | `/api/feeding-tasks/{id}/status` | ADMIN/任务负责人 | 未测 |  |
| D-19 | 查询全部投喂记录 | GET | `/api/feeding-records` | ADMIN/VOLUNTEER | 未测 |  |
| D-20 | 查询投喂记录详情 | GET | `/api/feeding-records/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| D-21 | 按任务查询投喂记录 | GET | `/api/feeding-records/by-shift/{shiftId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-22 | 按志愿者查询投喂记录 | GET | `/api/feeding-records/by-volunteer/{volunteerId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-23 | 提交投喂记录 | POST | `/api/feeding-records` | ADMIN/任务负责人 | 未测 |  |
| D-24 | 查询全部交接记录 | GET | `/api/handovers` | ADMIN/VOLUNTEER | 未测 |  |
| D-25 | 查询交接详情 | GET | `/api/handovers/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| D-26 | 按发起人查询交接 | GET | `/api/handovers/by-from/{fromVolunteerId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-27 | 按接收人查询交接 | GET | `/api/handovers/by-to/{toVolunteerId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-28 | 按状态查询交接 | GET | `/api/handovers/by-status/{status}` | ADMIN/VOLUNTEER | 未测 |  |
| D-29 | 按关联对象查询交接 | GET | `/api/handovers/by-related/{relatedType}/{relatedId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-30 | 提交交接 | POST | `/api/handovers` | ADMIN/任务负责人 | 未测 |  |
| D-31 | 确认交接 | PUT | `/api/handovers/{id}/confirm` | ADMIN/接收方 | 未测 |  |
| D-32 | 拒绝交接 | PUT | `/api/handovers/{id}/reject` | ADMIN/接收方 | 未测 |  |
| D-33 | 撤销交接 | PUT | `/api/handovers/{id}/cancel` | ADMIN/发起方 | 未测 |  |
| D-34 | 查询众筹项目 | GET | `/api/crowdfunding-projects` | 公开 | 未测 |  |
| D-35 | 查询众筹项目详情 | GET | `/api/crowdfunding-projects/{id}` | 公开 | 未测 |  |
| D-36 | 按状态查询众筹项目 | GET | `/api/crowdfunding-projects/by-status/{status}` | 公开 | 未测 |  |
| D-37 | 按猫查询众筹项目 | GET | `/api/crowdfunding-projects/by-cat/{catId}` | 公开 | 未测 |  |
| D-38 | 新增众筹项目 | POST | `/api/crowdfunding-projects` | ADMIN | 未测 |  |
| D-39 | 更新众筹项目 | PUT | `/api/crowdfunding-projects/{id}` | ADMIN | 未测 |  |
| D-40 | 更新众筹项目状态 | PUT | `/api/crowdfunding-projects/{id}/status` | ADMIN | 未测 |  |
| D-41 | 查询全部捐赠 | GET | `/api/donations` | ADMIN | 未测 |  |
| D-42 | 查询捐赠详情 | GET | `/api/donations/{id}` | ADMIN | 未测 |  |
| D-43 | 按项目查询捐赠 | GET | `/api/donations/by-project/{projectId}` | ADMIN | 未测 |  |
| D-44 | 按捐赠人查询 | GET | `/api/donations/by-donor/{donorUserId}` | 本人/ADMIN | 未测 |  |
| D-45 | 记录捐赠 | POST | `/api/donations` | 已登录 | 未测 |  |
| D-46 | 查询全部支出 | GET | `/api/expense-records` | ADMIN/VOLUNTEER | 未测 |  |
| D-47 | 查询支出详情 | GET | `/api/expense-records/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| D-48 | 按项目查询支出 | GET | `/api/expense-records/by-project/{projectId}` | ADMIN/VOLUNTEER | 未测 |  |
| D-49 | 查询已审核支出 | GET | `/api/expense-records/by-project/{projectId}/approved-expenses` | ADMIN/VOLUNTEER | 未测 |  |
| D-50 | 记录支出 | POST | `/api/expense-records` | ADMIN/VOLUNTEER | 未测 |  |
| D-51 | 审核支出 | PUT | `/api/expense-records/{id}/audit` | ADMIN | 未测 |  |
| D-52 | 查询项目财务公示 | GET | `/api/financial-disclosure/{projectId}` | 公开 | 未测 |  |
| D-53 | 查询财务公示摘要 | GET | `/api/financial-disclosure/summary` | 公开 | 未测 |  |
| D-54 | 查询统计快照 | GET | `/api/statistics-reports` | 已登录 | 未测 |  |
| D-55 | 按 ID 查询统计快照 | GET | `/api/statistics-reports/snapshot/{id}` | 已登录 | 未测 |  |
| D-56 | 按指标查询快照 | GET | `/api/statistics-reports/by-metric/{metricCode}` | 已登录 | 未测 |  |
| D-57 | 按维度查询快照 | GET | `/api/statistics-reports/by-dimension/{dimensionType}/{dimensionValue}` | 已登录 | 未测 |  |
| D-58 | 生成统计快照 | POST | `/api/statistics-reports/generate/{projectId}` | ADMIN | 未测 |  |

## 3. 问题详情

| 问题编号 | 接口编号 | 严重性 | 测试数据/前置条件 | 预期结果 | 实际结果 | 响应码/错误信息 | 附件/链接 | 当前状态 |
|---|---|---|---|---|---|---|---|---|
| BUG-D-001 |  | 阻塞/严重/一般 |  |  |  |  |  | 待修复 |

```text
问题编号：BUG-D-___
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
- [ ] 58 个接口均已填写状态。
- [ ] 每个不通过或阻塞项都有问题编号和复现信息。
- [ ] 已隐藏 Token、密码、数据库账号和服务器凭据。
