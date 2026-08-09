# D 组（功能点 17-21）：接口核验结果提交模板

对应文档：[D组_领养志愿者投喂财务接口文档.md](D组_领养志愿者投喂财务接口文档.md)

## 1. 核验基本信息

| 项目             | 填写内容                                                     |
| ---------------- | ------------------------------------------------------------ |
| 测试人           | 宋新悦                                                       |
| 测试日期         | 2026-08-07                                                   |
| 代码版本/Commit  | `07dbc8b`                                                    |
| 接口文档版本     | D组_领养志愿者投喂财务接口文档.md                            |
| 后端地址         | `http://localhost:5047`                                      |
| Oracle 环境      | Oracle 21c, PDB: pdb1, 用户: CAT_SYSTEM                      |
| 测试账号/角色    | a_group_admin (ADMIN) / a_group_volunteer (VOLUNTEER) / a_group_user (USER) |
| 是否使用演示数据 | 是                                                           |

| 项目          | 填写内容                                                                        |
| ----------- | --------------------------------------------------------------------------- |
| 测试人         | 尹佳玮                                                                         |
| 测试日期        | 2026-08-08                                                                  |
| 代码版本/Commit | feature/rescue-14-15-16 / 537c292                                           |
| 接口文档版本      | D组_领养志愿者投喂财务接口文档.md                                                         |
| 后端地址        | `http://localhost:5047`                                                     |
| Oracle 环境   | Oracle 21c, PDB: pdb1, 用户: CAT_SYSTEM                                       |
| 测试账号/角色     | a_group_admin (ADMIN) / a_group_volunteer (VOLUNTEER) / a_group_user (USER) |
| 是否使用演示数据    | 是                                                                           |
## 2. 接口核验结果

状态统一填写：`未测`、`通过`、`不通过`、`阻塞`。不通过或阻塞项必须填写问题详情。

### 领养流程 (D-01 ~ D-05)（宋新悦）

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| D-01 | 查询待审核领养申请 | GET | `/api/adoption-workflow/pending` | ADMIN | 通过 |  |
| D-02 | 查询领养回访汇总 | GET | `/api/adoption-workflow/visits` | ADMIN | 通过 | BUG-D-002 |
| D-03 | 提交领养申请 | POST | `/api/adoption-workflow/applications` | USER | 通过 | BUG-D-001 |
| D-04 | 审核领养申请 | POST | `/api/adoption-workflow/applications/{applicationId}/review` | ADMIN | 通过 |  |
| D-05 | 新增领养回访 | POST | `/api/adoption-workflow/applications/{applicationId}/visits` | ADMIN | 通过 |  |

### 志愿者流程 (D-06 ~ D-10)（尹佳玮）

| 编号   | 接口      | 方法   | URL                                                 | 测试角色      | 状态  | 问题编号 |
| ---- | ------- | ---- | --------------------------------------------------- | --------- | --- | ---- |
| D-06 | 查询志愿者看板 | GET  | `/api/volunteer-workflow/activity`                  | ADMIN     | 通过  |      |
| D-07 | 注册志愿者   | POST | `/api/volunteer-workflow/volunteers`                | ADMIN     | 通过  |      |
| D-08 | 新建志愿者排班 | POST | `/api/volunteer-workflow/shifts`                    | ADMIN     | 通过  |      |
| D-09 | 排班签到    | POST | `/api/volunteer-workflow/shifts/{shiftId}/checkins` | VOLUNTEER | 未测 |      |
| D-10 | 新增积分日志  | POST | `/api/volunteer-workflow/credit-logs`               | ADMIN     | 通过  |      |

### 投喂任务 (D-11 ~ D-18)（宋新悦）

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| D-11 | 查询全部投喂任务 | GET | `/api/feeding-tasks` | ADMIN | 通过 |  |
| D-12 | 查询投喂任务详情 | GET | `/api/feeding-tasks/{id}` | ADMIN | 通过 |  |
| D-13 | 按志愿者查询任务 | GET | `/api/feeding-tasks/by-volunteer/{volunteerId}` | ADMIN | 通过 |  |
| D-14 | 按点位查询任务 | GET | `/api/feeding-tasks/by-point/{pointId}` | ADMIN | 通过 |  |
| D-15 | 按状态查询任务 | GET | `/api/feeding-tasks/by-status/{status}` | ADMIN | 通过 |  |
| D-16 | 新增投喂任务 | POST | `/api/feeding-tasks` | ADMIN | 通过 |  |
| D-17 | 更新投喂任务 | PUT | `/api/feeding-tasks/{id}` | ADMIN | 通过 |  |
| D-18 | 更新投喂任务状态 | PUT | `/api/feeding-tasks/{id}/status` | ADMIN | 通过 |  |

### 投喂记录 (D-19 ~ D-23)（宋新悦）

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| D-19 | 查询全部投喂记录 | GET | `/api/feeding-records` | ADMIN | 通过 |  |
| D-20 | 查询投喂记录详情 | GET | `/api/feeding-records/{id}` | ADMIN | 通过 |  |
| D-21 | 按任务查询投喂记录 | GET | `/api/feeding-records/by-shift/{shiftId}` | ADMIN | 通过 |  |
| D-22 | 按志愿者查询投喂记录 | GET | `/api/feeding-records/by-volunteer/{volunteerId}` | ADMIN | 通过 |  |
| D-23 | 提交投喂记录 | POST | `/api/feeding-records` | ADMIN | 通过 |  |

### 交接记录 (D-24 ~ D-33)（宋新悦）

| 编号 | 接口               | 方法 | URL                                                   | 测试角色 | 状态 | 问题编号  |
| ---- | ------------------ | ---- | ----------------------------------------------------- | -------- | ---- | --------- |
| D-24 | 查询全部交接记录   | GET  | `/api/handovers`                                      | ADMIN    | 通过 | BUG-D-002 |
| D-25 | 查询交接详情       | GET  | `/api/handovers/{id}`                                 | ADMIN    | 通过 |           |
| D-26 | 按发起人查询交接   | GET  | `/api/handovers/by-from/{fromVolunteerId}`            | ADMIN    | 通过 | BUG-D-002 |
| D-27 | 按接收人查询交接   | GET  | `/api/handovers/by-to/{toVolunteerId}`                | ADMIN    | 通过 | BUG-D-002 |
| D-28 | 按状态查询交接     | GET  | `/api/handovers/by-status/{status}`                   | ADMIN    | 通过 | BUG-D-002 |
| D-29 | 按关联对象查询交接 | GET  | `/api/handovers/by-related/{relatedType}/{relatedId}` | ADMIN    | 通过 | BUG-D-002 |
| D-30 | 提交交接           | POST | `/api/handovers`                                      | ADMIN    | 通过 |           |
| D-31 | 确认交接           | PUT  | `/api/handovers/{id}/confirm`                         | ADMIN    | 通过 |           |
| D-32 | 拒绝交接           | PUT  | `/api/handovers/{id}/reject`                          | ADMIN    | 通过 |           |
| D-33 | 撤销交接           | PUT  | `/api/handovers/{id}/cancel`                          | ADMIN    | 通过 |           |
### 众筹项目(D-34 ~ D-40)（尹佳玮）

| 编号   | 接口          | 方法   | URL                                                                     | 测试角色            | 状态  | 问题编号 |
| ---- | ----------- | ---- | ----------------------------------------------------------------------- | --------------- | --- | ---- |
| D-34 | 查询众筹项目      | GET  | `/api/crowdfunding-projects`                                            | 公开              | 通过  |      |
| D-35 | 查询众筹项目详情    | GET  | `/api/crowdfunding-projects/{id}`                                       | 公开              | 通过  |      |
| D-36 | 按状态查询众筹项目   | GET  | `/api/crowdfunding-projects/by-status/{status}`                         | 公开              | 通过  |      |
| D-37 | 按猫查询众筹项目    | GET  | `/api/crowdfunding-projects/by-cat/{catId}`                             | 公开              | 通过  |      |
| D-38 | 新增众筹项目      | POST | `/api/crowdfunding-projects`                                            | ADMIN           | 通过  |      |
| D-39 | 更新众筹项目      | PUT  | `/api/crowdfunding-projects/{id}`                                       | ADMIN           | 通过  |      |
| D-40 | 更新众筹项目状态    | PUT  | `/api/crowdfunding-projects/{id}/status`                                | ADMIN           | 通过  |      |
###  捐赠(D-41 ~ D-45)（尹佳玮）

| 编号   | 接口          | 方法   | URL                                                                     | 测试角色            | 状态  | 问题编号 |
| ---- | ----------- | ---- | ----------------------------------------------------------------------- | --------------- | --- | ---- |
| D-41 | 查询全部捐赠      | GET  | `/api/donations`                                                        | ADMIN           | 通过  |      |
| D-42 | 查询捐赠详情      | GET  | `/api/donations/{id}`                                                   | ADMIN           | 通过  |      |
| D-43 | 按项目查询捐赠     | GET  | `/api/donations/by-project/{projectId}`                                 | ADMIN           | 通过  |      |
| D-44 | 按捐赠人查询      | GET  | `/api/donations/by-donor/{donorUserId}`                                 | 本人/ADMIN        | 通过  |      |
| D-45 | 记录捐赠        | POST | `/api/donations`                                                        | 已登录             | 通过  |      |
### 支出(D-46 ~ D-51)（尹佳玮）

| 编号   | 接口          | 方法   | URL                                                                     | 测试角色            | 状态  | 问题编号 |
| ---- | ----------- | ---- | ----------------------------------------------------------------------- | --------------- | --- | ---- |
| D-46 | 查询全部支出      | GET  | `/api/expense-records`                                                  | ADMIN/VOLUNTEER | 通过  |      |
| D-47 | 查询支出详情      | GET  | `/api/expense-records/{id}`                                             | ADMIN/VOLUNTEER | 通过  |      |
| D-48 | 按项目查询支出     | GET  | `/api/expense-records/by-project/{projectId}`                           | ADMIN/VOLUNTEER | 通过  |      |
| D-49 | 查询已审核支出     | GET  | `/api/expense-records/by-project/{projectId}/approved-expenses`         | ADMIN/VOLUNTEER | 通过  |      |
| D-50 | 记录支出        | POST | `/api/expense-records`                                                  | ADMIN/VOLUNTEER | 通过  |      |
| D-51 | 审核支出        | PUT  | `/api/expense-records/{id}/audit`                                       | ADMIN           | 通过  |      |
### 财务公示(D-52 ~ D-53)（尹佳玮）

| 编号   | 接口       | 方法  | URL                                     | 测试角色 | 状态  | 问题编号 |
| ---- | -------- | --- | --------------------------------------- | ---- | --- | ---- |
| D-52 | 查询项目财务公示 | GET | `/api/financial-disclosure/{projectId}` | 公开   | 通过  |      |
| D-53 | 查询财务公示摘要 | GET | `/api/financial-disclosure/summary`     | 公开   | 通过  |      |
### 统计快照(D-54 ~ D-58)（尹佳玮）
| 编号 | 接口               | 方法 | URL                                                          | 测试角色 | 状态 | 问题编号 |
| :--: | ------------------ | ---- | ------------------------------------------------------------ | -------- | ---- | -------- |
| D-54 | 查询统计快照       | GET  | `/api/statistics-reports`                                    | 已登录   | 通过 |          |
| D-55 | 按 ID 查询统计快照 | GET  | `/api/statistics-reports/snapshot/{id}`                      | 已登录   | 通过 |          |
| D-56 | 按指标查询快照     | GET  | `/api/statistics-reports/by-metric/{metricCode}`             | 已登录   | 通过 |          |
| D-57 | 按维度查询快照     | GET  | `/api/statistics-reports/by-dimension/{dimensionType}/{dimensionValue}` | 已登录   | 通过 |          |
| D-58 | 生成统计快照       | POST | `/api/statistics-reports/generate/{projectId}`               | ADMIN    | 通过 |          |


## 3. 问题详情

| 问题编号 | 接口编号 | 严重性 | 测试数据/前置条件 | 预期结果 | 实际结果 | 响应码/错误信息 | 附件/链接 | 当前状态 |
|---|---|---|---|---|---|---|---|---|
| BUG-D-001 | D-03 | 一般 | 普通用户使用 valid catId 提交领养申请 | 返回 200 并包含 applicationId | 200 OK，响应体为空，无法获取申请 ID | 无错误信息 |  | 待修复 |
| BUG-D-002 | D-02, D-24~D-29 | 一般 | 查询回访汇总/交接记录，conclusion、remark 等字段含中文 | 正常显示中文 | 中文显示为 ????? 乱码（D-02 conclusion 字段、D-24/26/27/28/29 remark 字段均受影响） | 200 OK |  | 待修复 |

```text
问题编号：BUG-D-001
接口编号：D-03
测试人/时间：宋新悦 / 2026-08-07
严重性：一般
前置条件：普通用户已登录，存在有效 catId
请求方法与 URL：POST http://localhost:5047/api/adoption-workflow/applications
请求头（隐藏 Token）：Authorization: Bearer <USER_TOKEN>
请求参数/请求体：{"catId": "demo-cat-campus-001"}
预期结果：返回 200，响应体包含 applicationId
实际结果：200 OK，响应体完全为空
响应状态码与响应体：200 OK，空响应体
复现步骤：
1. 使用普通用户 Token 调用 POST /api/adoption-workflow/applications
2. 观察响应体为空，无法获取 applicationId
3. 需额外调用 GET /pending 才能找到刚提交的申请
建议处理人：后端开发
复测结果：未修复

---

问题编号：BUG-D-002
接口编号：D-02, D-24, D-26, D-27, D-28, D-29
测试人/时间：宋新悦 / 2026-08-07
严重性：一般
前置条件：已有回访记录（含中文 conclusion）或交接记录（含中文 remark）
请求方法与 URL：
  GET http://localhost:5047/api/adoption-workflow/visits
  GET http://localhost:5047/api/handovers 等
请求头（隐藏 Token）：Authorization: Bearer <ADMIN_TOKEN>
预期结果：conclusion、remark 等中文字段正常显示
实际结果：中文显示为 "??????,?????"（conclusion）或 "????"（remark），受影响接口：D-02、D-24、D-26、D-27、D-28、D-29
响应状态码与响应体：200 OK，但中文字段显示为问号乱码
复现步骤：
1. 使用管理员 Token 调用 GET /api/adoption-workflow/visits
2. 观察 conclusion 字段显示为 "??????,?????"
3. 调用 GET /api/handovers，观察部分 remark 字段显示为 "????"
4. 推测为 Oracle NLS_LANG 编码问题，影响所有含中文的 VARCHAR2 字段
建议处理人：后端开发 / DBA
复测结果：未修复
```
## 4. 测试响应体结果
### 领养流程

D-01 200 OK

```json
[
    {
        "applicationId": "APP-OVSEIYTL",
        "catId": "demo-cat-campus-001",
        "catName": "图图",
        "applicantUserId": "user-normal-a-group",
        "applicantName": "a_group_user",
        "applyTime": "2026-08-07T23:06:06",
        "currentStatus": "PENDING",
        "reviewerUserId": null,
        "agreementNo": null,
        "confirmTime": null
    }
]
```

D-02 200 OK

```json
[
    {
        "visitId": "VIS-QXALKXQV",
        "applicationId": "APP-MMQDQ32B",
        "catId": "demo-cat-campus-001",
        "visitType": "INITIAL",
        "visitTime": "2026-08-07T23:19:51",
        "visitorUserId": "user-admin-a-group",
        "conclusion": "猫咪适应良好",
        "passFlag": 1,
        "currentStatus": "APPROVED"
    },
    {
        "visitId": "VIS-G3DQR35F",
        "applicationId": "APP-SBK5D0BT",
        "catId": "demo-cat-campus-001",
        "visitType": "INITIAL",
        "visitTime": "2026-08-07T21:02:18",
        "visitorUserId": "user-admin-a-group",
        "conclusion": "??????,?????",
        "passFlag": 1,
        "currentStatus": "APPROVED"
    }
]
```

D-03 200 OK（响应体为空）
D-04 204 No Content
D-05 200 OK（响应体为空）

### 志愿者流程

D-06 200 OK

```json
[

    {

        "volunteerId": "VOL-JXJMGVCJ",

        "userId": "user-volunteer-a-group",

        "userName": "a_group_volunteer",

        "activeStatus": "ACTIVE",

        "creditLevel": "L1",

        "serviceScore": 6,

        "shiftId": "SHIFT-EIIYJTAC",

        "shiftStatus": "COMPLETED",

        "planStartTime": "2026-08-09T09:00:00",

        "planEndTime": "2026-08-09T10:00:00"

    }

]
```
D-07/D-08/D-09/D-10 200 OK 响应体为空
### 领养流程
D-01 200 OK
```json
[
    {
        "applicationId": "APP-OVSEIYTL",
        "catId": "demo-cat-campus-001",
        "catName": "图图",
        "applicantUserId": "user-normal-a-group",
        "applicantName": "a_group_user",
        "applyTime": "2026-08-07T23:06:06",
        "currentStatus": "PENDING",
        "reviewerUserId": null,
        "agreementNo": null,
        "confirmTime": null
    }
]
```
D-02 200 OK
```json
[
    {
        "visitId": "VIS-QXALKXQV",
        "applicationId": "APP-MMQDQ32B",
        "catId": "demo-cat-campus-001",
        "visitType": "INITIAL",
        "visitTime": "2026-08-07T23:19:51",
        "visitorUserId": "user-admin-a-group",
        "conclusion": "猫咪适应良好",
        "passFlag": 1,
        "currentStatus": "APPROVED"
    },
    {
        "visitId": "VIS-G3DQR35F",
        "applicationId": "APP-SBK5D0BT",
        "catId": "demo-cat-campus-001",
        "visitType": "INITIAL",
        "visitTime": "2026-08-07T21:02:18",
        "visitorUserId": "user-admin-a-group",
        "conclusion": "??????,?????",
        "passFlag": 1,
        "currentStatus": "APPROVED"
    }
]
```
D-03 200 OK（响应体为空）
D-04 204 No Content
D-05 200 OK（响应体为空）

### 投喂任务
D-11 200 OK
```json
[
    {"shiftID":"99d6a479-611c-425d-905e-51ed83bb8eb4","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-08T08:00:00","planEndTime":"2026-08-08T12:00:00","shiftStatus":"ASSIGNED"},
    {"shiftID":"00c57b02-8cac-4d65-aaae-6d9e3eae34cb","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T23:00:00","planEndTime":"2026-08-08T02:00:00","shiftStatus":"COMPLETED"},
    {"shiftID":"dab5efa6-f53d-46f1-9b93-607991ba940a","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:05:28","planEndTime":"2026-08-07T23:05:28","shiftStatus":"ASSIGNED"},
    {"shiftID":"6df29035-cb35-48e4-b0de-be62fead90d2","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:05:28","planEndTime":"2026-08-07T23:05:28","shiftStatus":"ASSIGNED"},
    {"shiftID":"227e50e8-bd80-40c6-83a7-d1c9f9f6e7c3","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:04:46","planEndTime":"2026-08-07T23:04:46","shiftStatus":"ASSIGNED"},
    {"shiftID":"68c5528f-e486-4a4f-b7b9-3cbff72eb5c3","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:04:29","planEndTime":"2026-08-08T02:04:29","shiftStatus":"COMPLETED"},
    {"shiftID":"SHIFT-5NN43IFB","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:03:20","planEndTime":"2026-08-08T01:03:20","shiftStatus":"COMPLETED"}
]
```
D-12 200 OK
```json
{"shiftID":"99d6a479-611c-425d-905e-51ed83bb8eb4","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-08T08:00:00","planEndTime":"2026-08-08T12:00:00","shiftStatus":"ASSIGNED"}
```
D-13 200 OK
```json
[
    {"shiftID":"99d6a479-611c-425d-905e-51ed83bb8eb4","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-08T08:00:00","planEndTime":"2026-08-08T12:00:00","shiftStatus":"ASSIGNED"},
    {"shiftID":"227e50e8-bd80-40c6-83a7-d1c9f9f6e7c3","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:04:46","planEndTime":"2026-08-07T23:04:46","shiftStatus":"ASSIGNED"}
]
```
D-14 200 OK
```json
[
    {"shiftID":"99d6a479-611c-425d-905e-51ed83bb8eb4","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-08T08:00:00","planEndTime":"2026-08-08T12:00:00","shiftStatus":"ASSIGNED"},
    {"shiftID":"00c57b02-8cac-4d65-aaae-6d9e3eae34cb","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T23:00:00","planEndTime":"2026-08-08T02:00:00","shiftStatus":"COMPLETED"},
    {"shiftID":"dab5efa6-f53d-46f1-9b93-607991ba940a","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:05:28","planEndTime":"2026-08-07T23:05:28","shiftStatus":"ASSIGNED"},
    {"shiftID":"6df29035-cb35-48e4-b0de-be62fead90d2","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:05:28","planEndTime":"2026-08-07T23:05:28","shiftStatus":"ASSIGNED"},
    {"shiftID":"227e50e8-bd80-40c6-83a7-d1c9f9f6e7c3","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:04:46","planEndTime":"2026-08-07T23:04:46","shiftStatus":"ASSIGNED"},
    {"shiftID":"68c5528f-e486-4a4f-b7b9-3cbff72eb5c3","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:04:29","planEndTime":"2026-08-08T02:04:29","shiftStatus":"COMPLETED"},
    {"shiftID":"SHIFT-5NN43IFB","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:03:20","planEndTime":"2026-08-08T01:03:20","shiftStatus":"COMPLETED"}
]
```
D-15 200 OK
```json
[
    {"shiftID":"99d6a479-611c-425d-905e-51ed83bb8eb4","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-08T08:00:00","planEndTime":"2026-08-08T12:00:00","shiftStatus":"ASSIGNED"},
    {"shiftID":"dab5efa6-f53d-46f1-9b93-607991ba940a","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:05:28","planEndTime":"2026-08-07T23:05:28","shiftStatus":"ASSIGNED"},
    {"shiftID":"6df29035-cb35-48e4-b0de-be62fead90d2","volunteerID":"VOL-B6YFJHKH","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:05:28","planEndTime":"2026-08-07T23:05:28","shiftStatus":"ASSIGNED"},
    {"shiftID":"227e50e8-bd80-40c6-83a7-d1c9f9f6e7c3","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-07T21:04:46","planEndTime":"2026-08-07T23:04:46","shiftStatus":"ASSIGNED"}
]
```
D-16 201 Created
```json
{"shiftID":"22f1a8a5-c219-4654-970f-b79ca038f5d1","volunteerID":"VOL-Y79HJR52","pointID":"demo-point-library-east","backupVolunteerID":null,"planStartTime":"2026-08-10T08:00:00","planEndTime":"2026-08-10T09:00:00","shiftStatus":"PLANNED"}
```
D-17 204 No Content
D-18 200 OK
```json
{"message":"投喂任务状态更新成功。"}
```
### 投喂记录
D-19 200 OK
```json
[
    {"checkInID":"6d9143cc-f956-465c-a3c6-50bb8da9cd46","shiftID":"00c57b02-8cac-4d65-aaae-6d9e3eae34cb","checkInTime":"2026-08-07T23:31:23","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"},
    {"checkInID":"67056706-1a20-4196-b3ec-f2ef541316ed","shiftID":"68c5528f-e486-4a4f-b7b9-3cbff72eb5c3","checkInTime":"2026-08-07T21:04:29","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"},
    {"checkInID":"CHK-LOBYUYH0","shiftID":"SHIFT-5NN43IFB","checkInTime":"2026-08-07T21:03:20","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"}
]
```
D-20 200 OK
```json
{"checkInID":"6d9143cc-f956-465c-a3c6-50bb8da9cd46","shiftID":"00c57b02-8cac-4d65-aaae-6d9e3eae34cb","checkInTime":"2026-08-07T23:31:23","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"}
```
D-21 200 OK
```json
[
    {"checkInID":"6d9143cc-f956-465c-a3c6-50bb8da9cd46","shiftID":"00c57b02-8cac-4d65-aaae-6d9e3eae34cb","checkInTime":"2026-08-07T23:31:23","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"}
]
```
D-22 200 OK
```json
[
    {"checkInID":"6d9143cc-f956-465c-a3c6-50bb8da9cd46","shiftID":"00c57b02-8cac-4d65-aaae-6d9e3eae34cb","checkInTime":"2026-08-07T23:31:23","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"},
    {"checkInID":"67056706-1a20-4196-b3ec-f2ef541316ed","shiftID":"68c5528f-e486-4a4f-b7b9-3cbff72eb5c3","checkInTime":"2026-08-07T21:04:29","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"},
    {"checkInID":"CHK-LOBYUYH0","shiftID":"SHIFT-5NN43IFB","checkInTime":"2026-08-07T21:03:20","longitude":null,"latitude":null,"photoUrl":null,"distanceMeters":null,"checkInStatus":"CHECKED_IN"}
]
```
D-23 201 Created
```json
{"checkInID":"ee69f0f5-6217-4aa7-be19-0bccf7613c98","shiftID":"dab5efa6-f53d-46f1-9b93-607991ba940a","checkInTime":"2026-08-09T22:30:00","longitude":121.5065,"latitude":31.2821,"photoUrl":"https://example.com/feeding-checkin.jpg","distanceMeters":5.0,"checkInStatus":"CHECKED_IN"}
```
### 交接记录
D-24 200 OK
```json
[
    {"handoverID":"b09d14b0-3a67-4d71-ad54-5fadb997ee33","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T23:40:12","confirmTime":null,"handoverStatus":"CANCELLED","remark":"测试撤销"},
    {"handoverID":"633bb222-5f3b-459b-bd23-3c4c3f6e5573","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"dab5efa6-f53d-46f1-9b93-607991ba940a","applyTime":"2026-08-07T23:38:55","confirmTime":null,"handoverStatus":"REJECTED","remark":"测试拒绝"},
    {"handoverID":"5c2147f2-7437-4eb0-8109-40b87523ec23","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"99d6a479-611c-425d-905e-51ed83bb8eb4","applyTime":"2026-08-07T23:32:53","confirmTime":"2026-08-07T23:38:03","handoverStatus":"CONFIRMED","remark":"测试交接"},
    {"handoverID":"5d539933-1535-4a21-a255-65926b03e0b0","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"dab5efa6-f53d-46f1-9b93-607991ba940a","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"REJECTED","remark":"????"},
    {"handoverID":"6edde28b-95f5-4e46-b6e4-f114ec94b507","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"CANCELLED","remark":"????"},
    {"handoverID":"e574bcfc-a029-4513-bae5-5b28be113f93","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"227e50e8-bd80-40c6-83a7-d1c9f9f6e7c3","applyTime":"2026-08-07T21:05:11","confirmTime":"2026-08-07T21:05:11","handoverStatus":"CONFIRMED","remark":"????"}
]
```
D-25 200 OK
```json
{"handoverID":"b09d14b0-3a67-4d71-ad54-5fadb997ee33","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T23:40:12","confirmTime":null,"handoverStatus":"CANCELLED","remark":"测试撤销"}
```
D-26 200 OK
```json
[
    {"handoverID":"b09d14b0-3a67-4d71-ad54-5fadb997ee33","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T23:40:12","confirmTime":null,"handoverStatus":"CANCELLED","remark":"测试撤销"},
    {"handoverID":"633bb222-5f3b-459b-bd23-3c4c3f6e5573","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"dab5efa6-f53d-46f1-9b93-607991ba940a","applyTime":"2026-08-07T23:38:55","confirmTime":null,"handoverStatus":"REJECTED","remark":"测试拒绝"},
    {"handoverID":"5c2147f2-7437-4eb0-8109-40b87523ec23","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"99d6a479-611c-425d-905e-51ed83bb8eb4","applyTime":"2026-08-07T23:32:53","confirmTime":"2026-08-07T23:38:03","handoverStatus":"CONFIRMED","remark":"测试交接"},
    {"handoverID":"5d539933-1535-4a21-a255-65926b03e0b0","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"dab5efa6-f53d-46f1-9b93-607991ba940a","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"REJECTED","remark":"????"},
    {"handoverID":"6edde28b-95f5-4e46-b6e4-f114ec94b507","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"CANCELLED","remark":"????"},
    {"handoverID":"e574bcfc-a029-4513-bae5-5b28be113f93","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"227e50e8-bd80-40c6-83a7-d1c9f9f6e7c3","applyTime":"2026-08-07T21:05:11","confirmTime":"2026-08-07T21:05:11","handoverStatus":"CONFIRMED","remark":"????"}
]
```
D-27 200 OK
```json
[
    {"handoverID":"b09d14b0-3a67-4d71-ad54-5fadb997ee33","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T23:40:12","confirmTime":null,"handoverStatus":"CANCELLED","remark":"测试撤销"},
    {"handoverID":"633bb222-5f3b-459b-bd23-3c4c3f6e5573","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"dab5efa6-f53d-46f1-9b93-607991ba940a","applyTime":"2026-08-07T23:38:55","confirmTime":null,"handoverStatus":"REJECTED","remark":"测试拒绝"},
    {"handoverID":"5c2147f2-7437-4eb0-8109-40b87523ec23","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"99d6a479-611c-425d-905e-51ed83bb8eb4","applyTime":"2026-08-07T23:32:53","confirmTime":"2026-08-07T23:38:03","handoverStatus":"CONFIRMED","remark":"测试交接"},
    {"handoverID":"5d539933-1535-4a21-a255-65926b03e0b0","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"dab5efa6-f53d-46f1-9b93-607991ba940a","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"REJECTED","remark":"????"},
    {"handoverID":"6edde28b-95f5-4e46-b6e4-f114ec94b507","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"CANCELLED","remark":"????"},
    {"handoverID":"e574bcfc-a029-4513-bae5-5b28be113f93","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"227e50e8-bd80-40c6-83a7-d1c9f9f6e7c3","applyTime":"2026-08-07T21:05:11","confirmTime":"2026-08-07T21:05:11","handoverStatus":"CONFIRMED","remark":"????"}
]
```
D-28 200 OK
```json
[
    {"handoverID":"b09d14b0-3a67-4d71-ad54-5fadb997ee33","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T23:40:12","confirmTime":null,"handoverStatus":"CANCELLED","remark":"测试撤销"},
    {"handoverID":"6edde28b-95f5-4e46-b6e4-f114ec94b507","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"CANCELLED","remark":"????"}
]
```
D-29 200 OK
```json
[
    {"handoverID":"b09d14b0-3a67-4d71-ad54-5fadb997ee33","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T23:40:12","confirmTime":null,"handoverStatus":"CANCELLED","remark":"测试撤销"},
    {"handoverID":"6edde28b-95f5-4e46-b6e4-f114ec94b507","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"SHIFT","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-07T21:05:28","confirmTime":null,"handoverStatus":"CANCELLED","remark":"????"}
]
```
D-30 201 Created
```json
{"handoverID":"6a070023-ce58-42b6-ab9d-a97c2abe336f","fromVolunteerID":"VOL-B6YFJHKH","toVolunteerID":"VOL-Y79HJR52","handoverType":"TASK","relatedType":"SHIFT","relatedID":"6df29035-cb35-48e4-b0de-be62fead90d2","applyTime":"2026-08-09T23:09:40.6428676+08:00","confirmTime":null,"handoverStatus":"PENDING","remark":"新测试交接"}
```
D-31 200 OK
```json
{"message":"交接已确认，关联的投喂任务负责人已更新。"}
```
D-32 200 OK
```json
{"message":"交接已拒绝。"}
```
D-33 200 OK
```json
{"message":"交接已撤销。"}
```
### 众筹项目
D-34/D-36/D-37 200 OK
```json
[

    {

        "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",

        "catID": "demo-cat-campus-001",

        "title": "受伤猫咪医疗众筹测试项目",

        "targetAmount": 1000,

        "raisedAmount": 0,

        "startTime": "2026-08-08T09:00:00",

        "endTime": "2026-09-08T18:00:00",

        "projectStatus": "ACTIVE"

    }

]
```
D-35 200 OK
```json
{

    "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",

    "catID": "demo-cat-campus-001",

    "title": "受伤猫咪医疗众筹测试项目",

    "targetAmount": 1000,

    "raisedAmount": 0,

    "startTime": "2026-08-08T09:00:00",

    "endTime": "2026-09-08T18:00:00",

    "projectStatus": "ACTIVE"

}
```
D-38 201 Created
```json
{

	"projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",

    "catID": "demo-cat-campus-001",

    "title": "受伤猫咪医疗众筹测试项目",

    "targetAmount": 1000,

    "raisedAmount": 0,

    "startTime": "2026-08-08T09:00:00",

    "endTime": "2026-09-08T18:00:00",

    "projectStatus": "ACTIVE"

}
```
D-39 204 No Content
D-40 200 OK
```json
{
    "message": "众筹项目状态更新成功。"
}
```
### 捐赠
D-41/D-43/D-44 200 OK
```json
[
    {
        "donationID": "07362859-957f-4184-8a9e-1ff68fc77ff6",
        "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "donorUserID": "user-admin-a-group",
        "amount": 100,
        "payMethod": "WECHAT",
        "payTime": "2026-08-08T10:00:00",
        "publicFlag": 1
    }
]
```
D-42 200 OK
```json
{
    "donationID": "07362859-957f-4184-8a9e-1ff68fc77ff6",
    "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
    "donorUserID": "user-admin-a-group",
    "amount": 100,
    "payMethod": "WECHAT",
    "payTime": "2026-08-08T10:00:00",
    "publicFlag": 1
}
```
D-45 201 Created
```json
{

    "donationID": "07362859-957f-4184-8a9e-1ff68fc77ff6",

    "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",

    "donorUserID": "user-admin-a-group",

    "amount": 100,

    "payMethod": "WECHAT",

    "payTime": "2026-08-08T10:00:00",

    "publicFlag": 1

}
```
### 支出
D-46/D-48 200 OK
```json
[
    {
        "financeID": "2ec47ff1-6aae-49d3-902e-d4673dc48ba6",
        "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "recordType": "MEDICAL",
        "amount": 30,
        "invoiceUrl": "https://example.com/invoice-test.jpg",
        "auditUserID": null,
        "auditStatus": "PENDING",
        "publicTime": null
    }
]
```
D-47 200 OK
```json
{
    "financeID": "2ec47ff1-6aae-49d3-902e-d4673dc48ba6",
    "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
    "recordType": "MEDICAL",
    "amount": 30,
    "invoiceUrl": "https://example.com/invoice-test.jpg",
    "auditUserID": null,
    "auditStatus": "PENDING",
    "publicTime": null
}
```
D-49 200OK
```json
[
    {
        "financeID": "2ec47ff1-6aae-49d3-902e-d4673dc48ba6",
        "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "recordType": "MEDICAL",
        "amount": 30,
        "invoiceUrl": "https://example.com/invoice-test.jpg",
        "auditUserID": "user-admin-a-group",
        "auditStatus": "APPROVED",
        "publicTime": "2026-08-09T18:20:52"
    }
]
```
D-50 201 Created
```json
{

    "financeID": "2ec47ff1-6aae-49d3-902e-d4673dc48ba6",

    "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",

    "recordType": "MEDICAL",

    "amount": 30,

    "invoiceUrl": "https://example.com/invoice-test.jpg",

    "auditUserID": null,

    "auditStatus": "PENDING",

    "publicTime": null

}
```
D-51 200 OK
```json
{

    "message": "支出记录审核完成。"

}
```
### 财务公示
D-52 200OK
```json
{
    "project": {
        "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "catID": null,
        "title": "受伤猫咪医疗众筹测试项目-已更新",
        "targetAmount": 1500,
        "raisedAmount": 100,
        "startTime": "2026-08-08T09:00:00",
        "endTime": "2026-09-08T18:00:00",
        "projectStatus": "ACTIVE"
    },
    "targetAmount": 1500,
    "raisedAmount": 100,
    "totalExpense": 30,
    "netBalance": 70,
    "donationCount": 1,
    "donations": [
        {
            "donationID": "07362859-957f-4184-8a9e-1ff68fc77ff6",
            "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
            "donorUserID": "user-admin-a-group",
            "amount": 100,
            "payMethod": "WECHAT",
            "payTime": "2026-08-08T10:00:00",
            "publicFlag": 1
        }
    ],
    "expenses": [
        {
            "financeID": "2ec47ff1-6aae-49d3-902e-d4673dc48ba6",
            "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
            "recordType": "MEDICAL",
            "amount": 30,
            "invoiceUrl": "https://example.com/invoice-test.jpg",
            "auditUserID": "user-admin-a-group",
            "auditStatus": "APPROVED",
            "publicTime": "2026-08-09T18:20:52"
        }
    ]
}
```
D-53 200OK
```json
[
    {
        "project": {
            "projectID": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
            "catID": null,
            "title": "受伤猫咪医疗众筹测试项目-已更新",
            "targetAmount": 1500,
            "raisedAmount": 100,
            "startTime": "2026-08-08T09:00:00",
            "endTime": "2026-09-08T18:00:00",
            "projectStatus": "ACTIVE"
        },
        "targetAmount": 1500,
        "raisedAmount": 100,
        "totalExpense": 30,
        "netBalance": 70,
        "donationCount": 1,
        "donations": [],
        "expenses": []
    }
]
```
### 统计快照
D-54 200 OK
```json
[
    {
        "snapshotID": "afbdf4fd-fce4-49fb-9f13-07cdf143020a",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "NET_BALANCE",
        "metricValue": 70,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目净余额（捐赠-支出）"
    },
    {
        "snapshotID": "720cc3aa-bba2-4057-858d-748d9bc61f0a",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "TOTAL_EXPENSE",
        "metricValue": 30,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目已审核通过支出总额"
    },
    {
        "snapshotID": "0a4c708b-c3f0-4ddc-bb88-d94adc894547",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "TOTAL_DONATION",
        "metricValue": 100,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目累计捐赠总额"
    },
    {
        "snapshotID": "03ae6078-23d0-4113-8b89-4328768c7ca5",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "DONATION_COUNT",
        "metricValue": 1,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "COUNT",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目捐赠笔数"
    }
]
```
D-55 200 OK
```json
{
        "snapshotID": "afbdf4fd-fce4-49fb-9f13-07cdf143020a",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "NET_BALANCE",
        "metricValue": 70,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目净余额（捐赠-支出）"
    }
```
D-56 200 OK
```json[
    {
        "snapshotID": "0a4c708b-c3f0-4ddc-bb88-d94adc894547",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "TOTAL_DONATION",
        "metricValue": 100,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目累计捐赠总额"
    }
]
```
D-57 200 OK
```json
[
    {
        "snapshotID": "0a4c708b-c3f0-4ddc-bb88-d94adc894547",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "TOTAL_DONATION",
        "metricValue": 100,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目累计捐赠总额"
    },
    {
        "snapshotID": "03ae6078-23d0-4113-8b89-4328768c7ca5",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "DONATION_COUNT",
        "metricValue": 1,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "COUNT",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目捐赠笔数"
    },
    {
        "snapshotID": "afbdf4fd-fce4-49fb-9f13-07cdf143020a",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "NET_BALANCE",
        "metricValue": 70,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目净余额（捐赠-支出）"
    },
    {
        "snapshotID": "720cc3aa-bba2-4057-858d-748d9bc61f0a",
        "snapshotDate": "2026-08-08T00:00:00",
        "metricCode": "TOTAL_EXPENSE",
        "metricValue": 30,
        "dimensionType": "PROJECT",
        "dimensionValue": "059b7f6a-aacd-4323-9bd9-522fd262ed64",
        "unit": "CNY",
        "generateTime": "2026-08-08T12:12:17",
        "remark": "项目已审核通过支出总额"
    }
]
```
D-58 200 OK
```json
{

    "message": "项目统计报表已生成。",

    "projectId": "059b7f6a-aacd-4323-9bd9-522fd262ed64",

    "projectTitle": "受伤猫咪医疗众筹测试项目-已更新",

    "metrics": {

        "totalDonation": 100,

        "totalExpense": 30,

        "netBalance": 70,

        "donationCount": 1

    }

}
```

## 5. 提交前检查

- [x] 已填写测试人、日期、代码版本和数据库环境。
- [x] 58 个接口均已填写状态。（本测试覆盖 D-01 ~ D-33，共 33 个接口）
- [x] 每个不通过或阻塞项都有问题编号和复现信息。
- [x] 已隐藏 Token、密码、数据库账号和服务器凭据。