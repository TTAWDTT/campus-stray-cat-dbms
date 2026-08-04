# A 组（功能点 1-4）：接口核验结果提交模板

对应文档：[A组_用户与权限接口文档.md](A组_用户与权限接口文档.md)

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
| A-01 | 用户登录 | POST | `/api/auth/login` | 公开 | 未测 |  |
| A-02 | 获取当前用户 | GET | `/api/auth/me` | 已登录 | 未测 |  |
| A-03 | 登录状态校验 | GET | `/api/auth/check` | 已登录 | 未测 |  |
| A-04 | 退出登录 | POST | `/api/auth/logout` | 已登录 | 未测 |  |
| A-05 | 查询用户列表 | GET | `/api/users` | ADMIN | 未测 |  |
| A-06 | 查询用户详情 | GET | `/api/users/{id}` | 本人/ADMIN | 未测 |  |
| A-07 | 新增用户 | POST | `/api/users` | ADMIN | 未测 |  |
| A-08 | 编辑用户 | PUT | `/api/users/{id}` | ADMIN | 未测 |  |
| A-09 | 启用或停用用户 | PATCH | `/api/users/{id}/status` | ADMIN | 未测 |  |
| A-10 | 查询角色列表 | GET | `/api/roles` | ADMIN | 未测 |  |
| A-11 | 查询角色详情 | GET | `/api/roles/{id}` | ADMIN | 未测 |  |
| A-12 | 新增角色 | POST | `/api/roles` | ADMIN | 未测 |  |
| A-13 | 编辑角色 | PUT | `/api/roles/{id}` | ADMIN | 未测 |  |
| A-14 | 删除角色 | DELETE | `/api/roles/{id}` | ADMIN | 未测 |  |
| A-15 | 给用户分配角色 | POST | `/api/roles/assign` | ADMIN | 未测 |  |
| A-16 | 查询黑名单列表 | GET | `/api/blacklist` | ADMIN | 未测 |  |
| A-17 | 查询黑名单详情 | GET | `/api/blacklist/{id}` | ADMIN | 未测 |  |
| A-18 | 加入黑名单 | POST | `/api/blacklist` | ADMIN | 未测 |  |
| A-19 | 解除黑名单 | PATCH | `/api/blacklist/{id}/release` | ADMIN | 未测 |  |
| A-20 | 查询用户黑名单状态 | GET | `/api/blacklist/status/{userId}` | ADMIN/VOLUNTEER | 未测 |  |
| A-21 | 批量解除黑名单 | PATCH | `/api/blacklist/release/batch` | ADMIN | 未测 |  |

## 3. 问题详情

| 问题编号 | 接口编号 | 严重性 | 测试数据/前置条件 | 预期结果 | 实际结果 | 响应码/错误信息 | 附件/链接 | 当前状态 |
|---|---|---|---|---|---|---|---|---|
| BUG-A-001 |  | 阻塞/严重/一般 |  |  |  |  |  | 待修复 |

```text
问题编号：BUG-A-___
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
- [ ] 21 个接口均已填写状态。
- [ ] 每个不通过或阻塞项都有问题编号和复现信息。
- [ ] 已隐藏 Token、密码、数据库账号和服务器凭据。
