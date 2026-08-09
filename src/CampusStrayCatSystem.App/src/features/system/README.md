# 系统管理

管理员专用的用户、角色权限与领养黑名单页面。

## 页面边界

- `pages/SystemPage.tsx`：系统管理单页，包含三个 Tab 与用户、角色、黑名单弹窗。
- `system.types.ts`：前端数据模型与写入请求模型。
- `../../services/system.service.ts`：`/api/users`、`/api/Roles`、`/api/blacklist` 的接口适配。

审计日志和系统设置尚无可用读取接口，当前不在此页面伪造展示。
