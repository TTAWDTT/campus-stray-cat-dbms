# A 组（功能点 1-4）：接口核验结果

对应文档：[A组_用户与权限接口文档.md](../A组_用户与权限接口文档.md)

## 1. 核验基本信息

| 项目 | 填写内容 |
|---|---|
| 测试人 | 赵晴、徐千顺（B 组） |
| 测试日期 | 2026-08-08、2026-08-09 |
| 代码版本/Commit | `537c292be1c7dd38aa15bd9d06ccbc82fdf937db` |
| 接口文档版本 | 同 Commit `537c292` 中的 `docs/A组_用户与权限接口文档.md` |
| 后端地址 | `http://localhost:5047` |
| Oracle 环境 | 本地 Docker Oracle XE 21c，服务 `XEPDB1`；数据库账号及连接凭据已隐藏 |
| 测试账号/角色 | `a_group_admin` / ADMIN、`a_group_volunteer` / VOLUNTEER、`a_group_user` / USER、`a_group_disabled` / DISABLED；密码已隐藏 |
| 是否使用演示数据 | 是；执行最新版建表、约束、视图、存储过程和 `a_group_demo_data.sql` |

## 2. 接口核验结果

状态统一填写：`未测`、`通过`、`不通过`、`阻塞`。不通过或阻塞项必须填写问题详情。

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| A-01 | 用户登录 | POST | `/api/auth/login` | 公开 | 不通过 | BUG-A-001 |
| A-02 | 获取当前用户 | GET | `/api/auth/me` | 已登录 | 通过 |  |
| A-03 | 登录状态校验 | GET | `/api/auth/check` | 已登录 | 不通过 | BUG-A-002 |
| A-04 | 退出登录 | POST | `/api/auth/logout` | 已登录 | 通过 |  |
| A-05 | 查询用户列表 | GET | `/api/users` | ADMIN | 通过 |  |
| A-06 | 查询用户详情 | GET | `/api/users/{id}` | 本人/ADMIN | 通过 |  |
| A-07 | 新增用户 | POST | `/api/users` | ADMIN | 通过 |  |
| A-08 | 编辑用户 | PUT | `/api/users/{id}` | ADMIN | 通过 |  |
| A-09 | 启用或停用用户 | PATCH | `/api/users/{id}/status` | ADMIN | 通过 |  |
| A-10 | 查询角色列表 | GET | `/api/roles` | ADMIN | 通过 |  |
| A-11 | 查询角色详情 | GET | `/api/roles/{id}` | ADMIN | 通过 |  |
| A-12 | 新增角色 | POST | `/api/roles` | ADMIN | 不通过 | BUG-A-003 |
| A-13 | 编辑角色 | PUT | `/api/roles/{id}` | ADMIN | 通过 |  |
| A-14 | 删除角色 | DELETE | `/api/roles/{id}` | ADMIN | 通过 |  |
| A-15 | 给用户分配角色 | POST | `/api/roles/assign` | ADMIN | 不通过 | BUG-A-006 |
| A-16 | 查询黑名单列表 | GET | `/api/blacklist` | ADMIN | 通过 |  |
| A-17 | 查询黑名单详情 | GET | `/api/blacklist/{id}` | ADMIN | 通过 |  |
| A-18 | 加入黑名单 | POST | `/api/blacklist` | ADMIN | 不通过 | BUG-A-004、BUG-A-005 |
| A-19 | 解除黑名单 | PATCH | `/api/blacklist/{id}/release` | ADMIN | 不通过 | BUG-A-007 |
| A-20 | 查询用户黑名单状态 | GET | `/api/blacklist/status/{userId}` | ADMIN/VOLUNTEER | 通过 |  |
| A-21 | 批量解除黑名单 | PATCH | `/api/blacklist/release/batch` | ADMIN | 通过 |  |

## 3. 问题详情

| 问题编号 | 接口编号 | 严重性 | 测试数据/前置条件 | 预期结果 | 实际结果 | 响应码/错误信息 | 附件/链接 | 当前状态 |
|---|---|---|---|---|---|---|---|---|
| BUG-A-001 | A-01 | 一般 | 公开接口；JSON 中 `username`、`password` 均为空字符串 | 400，响应 `{"message":"Username 与 Password 均为必填。"}` | 状态码正确，但返回 ASP.NET ValidationProblemDetails，缺少文档约定的 `message` | 400；`errors.Password`、`errors.Username` 为英文必填错误 | [CLI 日志](B组测试证据/B组_A组接口互测_2026-08-08.log) | 待修复 |
| BUG-A-002 | A-03 | 一般 | 不携带 Authorization 请求头 | 401，响应含 `{"authenticated":false,...}` | 返回 401 空响应体，前端无法按文档读取 `authenticated` | 401；Body 长度为 0 | [CLI 日志](B组测试证据/B组_A组接口互测_2026-08-08.log) | 待修复 |
| BUG-A-003 | A-12 | 严重 | ADMIN Token；动态取得已有角色 ID，并分别测试同名角色、重复 RoleID、缺少 RoleID | 同名或重复 ID 返回 409；缺少 ID 返回 400；不泄露数据库异常 | 同名角色返回 200 并落库；重复 ID 和缺少 ID 均返回 500，并向客户端返回 Oracle 异常 | 200；500/ORA-00001；500/ORA-01400 | [原始日志](B组测试证据/B组_A组接口互测_2026-08-08.log)、[补充日志](B组测试证据/B组_A组补充问题互测_2026-08-09.log) | 待修复 |
| BUG-A-004 | A-18 | 严重 | ADMIN Token；用户存在；`reasonType`、`reasonDetail` 合法；省略文档标为可选的 `applicationId` | 201 创建成功 | 在进入业务逻辑前被模型校验拒绝，将 `ApplicationId` 当作必填字段 | 400；`errors.ApplicationId = The ApplicationId field is required.` | [CLI 日志](B组测试证据/B组_A组接口互测_2026-08-08.log) | 待修复 |
| BUG-A-005 | A-18 | 一般 | 使用空白 `applicationId` 绕过 BUG-A-004 后成功创建黑名单 | 201，响应体包含黑名单 ID 和用户 ID | 201 响应体仅包含 `message`、`userId`；黑名单 ID 只出现在 Location 响应头 | 201；响应体缺少 `blacklistId` | [CLI 日志](B组测试证据/B组_A组接口互测_2026-08-08.log) | 待修复 |
| BUG-A-006 | A-15 | 严重 | ADMIN Token；测试用户由 A-07 实际创建；先分配 VOLUNTEER，再恢复 USER | 每次角色更新与一条 `UPDATE_ROLE` 审计记录在同一事务提交 | 两次角色更新均持久化，但该用户的审计记录总数和 `UPDATE_ROLE` 记录数均为 0 | API 两次均为 204；Oracle 审计计数为 0 | [Oracle 审计核验](B组测试证据/B组_A组A15审计核验_2026-08-08.log) | 待修复 |
| BUG-A-007 | A-19 | 一般 | ADMIN Token；通过黑名单列表响应动态取得一条 `RELEASED` 记录；请求体为 `{}` | `releaseReason` 为可选字段，应进入业务判断并返回 409 已解除 | 模型校验将 `ReleaseReason` 当作必填字段，业务判断前返回 400；补充该字段后才返回 409 | 400；`errors.ReleaseReason = The ReleaseReason field is required.` | [补充日志](B组测试证据/B组_A组补充问题互测_2026-08-09.log) | 待修复 |

```text
问题编号：BUG-A-001
接口编号：A-01
测试人/时间：赵晴、徐千顺 / 2026-08-08
严重性：一般
前置条件：后端与 Oracle 正常运行；登录接口为公开接口。
请求方法与 URL：POST http://localhost:5047/api/auth/login
请求头（隐藏 Token）：Content-Type: application/json
请求参数/请求体：{"username":"","password":""}
预期结果：400；{"message":"Username 与 Password 均为必填。"}
实际结果：400 ValidationProblemDetails，errors 中分别返回 Password、Username 英文必填错误，没有 message 字段。
响应状态码与响应体：400；{"title":"One or more validation errors occurred.","status":400,"errors":{"Password":["The Password field is required."],"Username":["The Username field is required."]},...}
复现步骤：
1. 在 Postman 新建 POST /api/auth/login。
2. Body 选择 raw / JSON，发送空 username 与 password。
3. 对照文档检查状态码和响应体，而非只检查 400。
附件/日志：B组测试证据/B组_A组接口互测_2026-08-08.log，A-01-03。
建议处理人：A 组认证接口负责人。
复测结果：未修复
```

```text
问题编号：BUG-A-002
接口编号：A-03
测试人/时间：赵晴、徐千顺 / 2026-08-08
严重性：一般
前置条件：后端正常运行；请求不携带 Token。
请求方法与 URL：GET http://localhost:5047/api/auth/check
请求头（隐藏 Token）：不携带 Authorization。
请求参数/请求体：无。
预期结果：401；响应体含 authenticated=false 及错误说明。
实际结果：401；响应体为空。
响应状态码与响应体：401；Body 长度 0。
复现步骤：
1. 在 Postman 新建 GET /api/auth/check。
2. Authorization 选择 No Auth。
3. 发送并检查 Body；状态码虽为 401，但响应体不符合接口文档。
附件/日志：B组测试证据/B组_A组接口互测_2026-08-08.log，A-03-02。
建议处理人：A 组认证接口负责人。
复测结果：未修复
```

```text
问题编号：BUG-A-003
接口编号：A-12
测试人/时间：赵晴、徐千顺 / 2026-08-08、2026-08-09
严重性：严重
前置条件：使用 ADMIN Token；通过 GET /api/roles 的实际响应动态取得已有 RoleID；另有已成功创建的 VET 测试角色。
请求方法与 URL：POST http://localhost:5047/api/roles
请求头（隐藏 Token）：Authorization: Bearer <已隐藏>；Content-Type: application/json
请求参数/请求体：用例一更换 RoleID 后重复 roleName=VET；用例二使用 API 返回的已有 RoleID；用例三省略 roleID、只传 roleName=VET。
预期结果：同名角色和重复 RoleID 返回 409；缺少 RoleID 返回可读的 400；均不向客户端泄露数据库异常。
实际结果：同名角色返回 200 并实际落库；重复 RoleID 返回 500/ORA-00001；缺少 RoleID 返回 500/ORA-01400，两个 500 响应均包含 OracleException 调用栈。
响应状态码与响应体：200，重复 VET 角色对象；500，包含 ORA-00001；500，包含 ORA-01400。
复现步骤：
1. 以管理员创建一个 VET 角色；更换 RoleID、保持 RoleName=VET 再次提交，返回 200，且按新 ID 可以查到。
2. 调用 GET /api/roles 动态取得一个已有 RoleID，将该 ID 用于 POST /api/roles，返回 500/ORA-00001。
3. POST /api/roles 时省略 roleID、只提交合法 VET 角色名，返回 500/ORA-01400。
4. 检查两个 500 响应体，均能看到 OracleException 和 ORA 错误信息。
附件/日志：B组测试证据/B组_A组接口互测_2026-08-08.log，A-12-01、A-12-03；B组测试证据/B组_A组补充问题互测_2026-08-09.log；对应 JUnit 文件同目录。
建议处理人：A 组角色权限负责人；使用包含必填 RoleID、RoleName 校验的请求 DTO，创建前检查 ID 和名称冲突，并将唯一约束冲突映射为 409。
复测结果：未修复
```

```text
问题编号：BUG-A-004
接口编号：A-18
测试人/时间：赵晴、徐千顺 / 2026-08-08
严重性：严重
前置条件：使用 ADMIN Token；目标用户存在且当前无有效黑名单。
请求方法与 URL：POST http://localhost:5047/api/blacklist
请求头（隐藏 Token）：Authorization: Bearer <已隐藏>；Content-Type: application/json
请求参数/请求体：{"userId":"user-admin-a-group","reasonType":"OTHER","reasonDetail":"验证 applicationId 可选契约"}
预期结果：applicationId 为文档标注的可选字段，省略时返回 201。
实际结果：模型校验将 ApplicationId 当作必填字段，返回 400，未进入创建逻辑。
响应状态码与响应体：400；{"title":"One or more validation errors occurred.","status":400,"errors":{"ApplicationId":["The ApplicationId field is required."]},...}
复现步骤：
1. 使用管理员 Token。
2. 发送合法 userId、reasonType、reasonDetail，但不发送 applicationId。
3. 对照文档检查；实际返回 400 而不是 201。
附件/日志：B组测试证据/B组_A组接口互测_2026-08-08.log，A-18-06A。
建议处理人：A 组黑名单负责人；将 DTO 的 ApplicationId 声明为可空并复核隐式 Required。
复测结果：未修复
```

```text
问题编号：BUG-A-005
接口编号：A-18
测试人/时间：赵晴、徐千顺 / 2026-08-08
严重性：一般
前置条件：使用 ADMIN Token；目标用户存在且无有效黑名单；为绕过 BUG-A-004，applicationId 传空白字符串。
请求方法与 URL：POST http://localhost:5047/api/blacklist
请求头（隐藏 Token）：Authorization: Bearer <已隐藏>；Content-Type: application/json
请求参数/请求体：动态 userId；reasonType=OTHER；合法 reasonDetail；applicationId=" "。
预期结果：201；响应体包含 blacklistId、userId。
实际结果：201；响应体只有 message、userId，blacklistId 只存在于 Location 响应头。
响应状态码与响应体：201；{"message":"用户 '<动态测试用户>' 已加入黑名单","userId":"<动态用户 ID>"}
复现步骤：
1. 使用管理员 Token 和不存在有效黑名单的用户。
2. POST /api/blacklist 创建记录。
3. 检查 201 响应体；找不到文档约定的 blacklistId。
4. 可从 Location 头提取 ID 并继续 A-19，但这不符合响应体契约。
附件/日志：B组测试证据/B组_A组接口互测_2026-08-08.log，A-18-06。
建议处理人：A 组黑名单负责人。
复测结果：未修复
```

```text
问题编号：BUG-A-006
接口编号：A-15
测试人/时间：赵晴、徐千顺 / 2026-08-08
严重性：严重
前置条件：使用 ADMIN Token；测试用户由 A-07 创建接口的真实响应获得，初始角色为 USER。
请求方法与 URL：POST http://localhost:5047/api/roles/assign
请求头（隐藏 Token）：Authorization: Bearer <已隐藏>；Content-Type: application/json
请求参数/请求体：先传动态 userId 与 role-volunteer-a-group，再传同一 userId 与 role-user-a-group 恢复现场。
预期结果：两次均更新成功，且每次角色更新与对应 UPDATE_ROLE 审计记录在同一事务提交。
实际结果：两次 API 均返回 204，GET 回查也确认角色持久化；但 LOG_AUDITTRAILS 中该用户的审计记录总数为 0。
响应状态码与响应体：204、204，响应体为空；UPDATE_ROLE 审计计数=0，全部审计计数=0。
复现步骤：
1. 以管理员调用 A-07 创建测试用户，保存响应中的真实 userId。
2. 调用 A-15 将该用户分配为 VOLUNTEER，并以 A-06 GET 回查角色。
3. 再调用 A-15 恢复为 USER。
4. 在 Oracle 查询 LOG_AUDITTRAILS 中该 RECORDID 的 UPDATE_ROLE 记录，实际为 0。
5. 核查调用链：RolesController 调用 RoleRepository.AssignRole；仓储只直接 UPDATE SYS_USERS，未调用 SP_ASSIGN_USER_ROLE，也未写审计表。
附件/日志：B组测试证据/B组_A组A15审计核验_2026-08-08.log；Newman 日志 A-15-01、A-15-07、A-15-08。
建议处理人：A 组角色权限负责人；让后端实际调用 SP_ASSIGN_USER_ROLE，传入 JWT 中的操作人 ID，并保证更新与审计原子提交。
复测结果：未修复
```

```text
问题编号：BUG-A-007
接口编号：A-19
测试人/时间：赵晴、徐千顺 / 2026-08-09
严重性：一般
前置条件：使用 ADMIN Token；通过 GET /api/blacklist?status=RELEASED 的实际响应动态取得一条已解除黑名单记录 ID。
请求方法与 URL：PATCH http://localhost:5047/api/blacklist/{动态取得的已解除记录 ID}/release
请求头（隐藏 Token）：Authorization: Bearer <已隐藏>；Content-Type: application/json
请求参数/请求体：{}
预期结果：接口文档规定 releaseReason 可选；省略后应进入记录状态判断并返回 409，说明记录已经解除。
实际结果：ASP.NET 模型校验把 ReleaseReason 当作必填字段，在业务判断前返回 400；同一记录补充 releaseReason 后才返回正确的 409。
响应状态码与响应体：400 ValidationProblemDetails；errors.ReleaseReason=["The ReleaseReason field is required."]。
复现步骤：
1. 以管理员查询 status=RELEASED 的黑名单列表，从响应取得一条真实 blacklistId。
2. 对该 ID 调用 PATCH /api/blacklist/{id}/release，请求体发送 {}，实际返回 400 必填错误。
3. 对同一 ID 再次请求并传入 releaseReason，实际返回 409，证明空对象在到达业务状态判断前已被模型校验拦截。
附件/日志：B组测试证据/B组_A组补充问题互测_2026-08-09.log；B组测试证据/B组_A组补充问题互测_2026-08-09.junit.xml。
建议处理人：A 组黑名单负责人；将 ReleaseBlacklistDto.ReleaseReason 改为 string?，或将接口文档同步改为必填。
复测结果：未修复
```

## 4. 提交前检查

- [x] 已填写测试人、日期、代码版本和数据库环境。
- [x] 21 个接口均已填写状态。
- [x] 每个不通过项都有问题编号和复现信息。
- [x] 已隐藏 Token、密码、数据库账号和服务器凭据。
