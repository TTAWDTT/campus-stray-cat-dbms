# 前端页面分工与开发说明

> 本文档用于组长组统一开发前端。前端不再按照 A/B/C/D 后端小组划分，而是按照页面和用户流程组织。

## 1. 技术栈

- React 18 + TypeScript
- Vite
- React Router
- Zustand：登录用户、角色菜单和全局界面状态
- Axios：统一请求、JWT 注入和 401 处理
- Less/CSS Modules
- `animal-island-ui`：Button、Card、Table、Form、Modal、Drawer、Tag、Progress、Tabs 等组件
- 后端：ASP.NET Core .NET 8 + Oracle + JWT

开发环境使用 Vite `/api` 代理到 ASP.NET Core；生产环境由 ASP.NET Core 托管构建后的 `wwwroot` 静态文件。

## 2. 视觉规范

项目采用“暖色校园自然风 + 轻量运营台”方向：

- 米白背景、棕色文字、薄荷绿主色
- 黄色表示提醒，红色表示危险或紧急状态
- 大圆角卡片、胶囊按钮和输入框
- 使用 Nunito + Noto Sans SC
- 桌面端使用侧边导航或顶部导航，移动端使用底部导航
- 列表在移动端改为卡片，筛选项放入 Drawer
- TNR 使用时间线，紧急上报使用醒目的状态卡片，财务页面保持清晰克制

## 3. 页面划分

### 页面一：登录与个人中心

接口：

- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/auth/check`
- `POST /api/auth/logout`
- `GET /api/users/{id}`

按钮和操作：登录、退出登录、刷新登录状态、查看或编辑个人信息。

登录成功后保存 JWT，并由 Axios 自动添加 `Authorization: Bearer <token>`。

### 页面二：首页仪表盘

首页调用各业务模块的查询接口进行汇总，不单独新增业务接口。

展示内容：校园猫咪数量、最近目击、进行中 TNR、待处理医疗提醒、待审核领养、志愿者排班、紧急上报和财务概况。

按钮：查看猫咪档案、记录目击、打开待办、进入救助中心、进入财务公示、查看个人任务。

根据普通用户、志愿者、兽医和管理员显示不同数据块。

### 页面三：猫咪档案

接口：

- `/api/cats`
- `/api/cats/{catId}`
- `/api/cats/{catId}/photos`
- `/api/cats/{catId}/photos/{photoId}/primary`
- `/api/cats/{catId}/photos/{photoId}/feature`
- `/api/naming-votes/cats/{catId}/candidates`
- `/api/naming-votes/candidates/{candidateId}/vote`
- `/api/naming-votes/candidates/{candidateId}/winner`

按钮：新增猫咪、编辑档案、归档、查看详情、上传照片、设置主图、删除照片、发布候选名、投票、确定获胜名称。

详情页使用 Tabs：基本档案、照片特征、目击记录、医疗历史、TNR 记录、命名投票、领养和众筹。

### 页面四：校园地图与目击

接口：

- `/api/campus-areas`
- `/api/campus-areas/roots`
- `/api/campus-areas/hierarchy`
- `/api/campus-areas/{id}/children`
- `/api/service-points`
- `/api/nest-maintenance-records`
- `/api/cat-sightings`
- `/api/cat-sightings/{id}`
- `/api/cat-sightings/recent/by-cat/{catId}`

按钮：查看区域、新增/编辑/删除区域、新增服务点、维护猫窝、记录目击、上传现场照片、按猫/区域/时间筛选、编辑或删除目击记录。

移动端固定显示“记录目击”主按钮。旧版 `/api/areas` 不作为前端入口。

### 页面五：救助中心

包含 TNR、医疗、医疗提醒、紧急上报和失踪预警五个 Tab。

主要接口：

- `/api/TnrCases`、`/api/TnrCases/{id}/status`
- `/api/TnrStatusLogs/case/{caseId}`
- `/api/MedHealthRecords`
- `/api/MedReminder`
- `/api/MedReminder/{reminderId}/sent`
- `/api/MedReminder/{reminderId}/complete`
- `/api/EmergencyReports`
- `/api/EmergencyReports/{reportId}/assign`
- `/api/EmergencyReports/{reportId}/status`
- `/api/MissingAlerts`
- `/api/MissingAlerts/sightings`
- `/api/MissingAlerts/{alertId}/status`

按钮：新建案例、编辑、更新状态、查看状态日志、新增医疗记录、新增提醒、标记已发送/完成、提交紧急上报、分配处理人、更新处理结果、发布失踪预警、补充最后目击、标记寻回或关闭。

TNR 使用“发现 → 捕捉 → 绝育 → 恢复 → 放归”时间线；紧急等级使用红、橙、黄标签。

### 页面六：领养、志愿者与投喂

领养接口：

- `/api/adoption-workflow/pending`
- `/api/adoption-workflow/visits`
- `/api/adoption-workflow/applications`
- `/api/adoption-workflow/applications/{applicationId}/review`
- `/api/adoption-workflow/applications/{applicationId}/visits`

志愿者接口：

- `/api/volunteer-workflow/activity`
- `/api/volunteer-workflow/volunteers`
- `/api/volunteer-workflow/shifts`
- `/api/volunteer-workflow/shifts/{shiftId}/checkins`
- `/api/volunteer-workflow/credit-logs`

投喂和交接接口：

- `/api/feeding-tasks`
- `/api/feeding-records`
- `/api/handovers`
- `/api/handovers/{id}/confirm`
- `/api/handovers/{id}/reject`
- `/api/handovers/{id}/cancel`

按钮：提交领养申请、审核通过/驳回、添加回访、注册志愿者、新建排班、签到、上传签到照片、查看积分、新增投喂任务、更新任务状态、提交投喂记录、发起/确认/拒绝/撤销交接。

当前领养接口没有“查询当前用户本人申请”的接口，提交接口也没有返回申请编号，正式开发前建议补充 `GET /api/adoption-workflow/my-applications` 并返回 `applicationId`。

### 页面七：财务公示

接口：

- `/api/crowdfunding-projects`
- `/api/donations`
- `/api/expense-records`
- `/api/expense-records/{id}/audit`
- `/api/financial-disclosure/{projectId}`
- `/api/financial-disclosure/summary`
- `/api/statistics-reports`
- `/api/statistics-reports/generate/{projectId}`

按钮：发起众筹、查看项目、我要捐赠、查看财务公示、记录支出、审核支出、生成统计快照。

页面突出项目金额、捐赠流水、审核状态、已筹进度和公开支出明细。

### 页面八：系统管理

用户接口：`/api/users`、`/api/users/{id}/status`。

角色接口：`/api/roles`、`/api/roles/{id}`、`/api/roles/assign`。

黑名单接口：`/api/blacklist`、`/api/blacklist/{id}/release`、`/api/blacklist/status/{userId}`、`/api/blacklist/release/batch`。

按钮：查询、重置、新增用户、编辑、启用/停用、分配角色、新增/编辑/删除角色、加入黑名单、查看原因、解除黑名单、批量解除。

加入和解除黑名单必须使用确认弹窗。该页面仅管理员可见。

## 4. 角色菜单

### 普通用户

首页、猫咪档案、校园地图、目击打卡、命名投票、领养申请、紧急上报、失踪预警、众筹与捐赠、个人中心。

### 志愿者

在普通用户基础上增加救助中心、志愿者看板、投喂任务、签到、交接、领养审核和猫咪/区域维护。

### 兽医

重点显示猫咪档案、TNR、医疗记录和医疗提醒。

### 管理员

显示全部页面，并拥有用户、角色、黑名单、财务审核、统计快照和志愿者注册权限。

前端菜单隐藏只用于改善交互，最终权限仍以服务端 JWT 和 `[Authorize]` 为准。

## 5. 开发顺序

1. 建立 React + Vite 工程并接入 `animal-island-ui`。
2. 完成登录、路由守卫、角色菜单和 Axios 请求封装。
3. 完成首页仪表盘、猫咪档案、猫咪详情和校园目击。
4. 完成救助中心。
5. 完成领养、志愿者、投喂和交接。
6. 完成财务公示。
7. 完成系统管理。
8. 最后统一处理加载、空状态、异常提示、移动端细节和接口契约问题。
