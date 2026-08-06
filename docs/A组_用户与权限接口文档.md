# A 组（功能点 1-4）：用户与权限接口测试文档

## 1. 公共约定

| 项目 | 约定 |
|---|---|
| 基础地址 | `http://localhost:5047` |
| 认证方式 | 除登录接口外，请求头携带 `Authorization: Bearer <token>` |
| 数据格式 | 请求和响应均使用 `application/json` |
| 时间格式 | ISO 8601，例如 `2026-08-04T10:20:30Z` |
| 用户状态 | `ACTIVE`、`DISABLED` |
| 验证状态 | `VERIFIED`、`UNVERIFIED` |
| 角色编码 | `ADMIN`、`VOLUNTEER`、`USER`、`VET` |
| 黑名单状态 | `ACTIVE`、`RELEASED` |
| 黑名单原因类型 | `ABANDONMENT`、`ANIMAL_ABUSE`、`FALSE_INFORMATION`、`OTHER` |

登录成功后将响应中的 `token` 放入后续请求的 `Authorization` 请求头。用户 ID、角色 ID 和密码哈希由服务端或数据库生成，接口不接受客户端伪造当前操作人身份。

## 2. 接口总览

| 功能 | 方法 | URL | 权限 |
|---|---|---|---|
| 登录 | POST | `/api/auth/login` | 公开 |
| 当前用户 | GET | `/api/auth/me` | 已登录 |
| 登录状态校验 | GET | `/api/auth/check` | 已登录 |
| 退出登录 | POST | `/api/auth/logout` | 已登录 |
| 用户列表 | GET | `/api/users` | 管理员 |
| 用户详情 | GET | `/api/users/{id}` | 本人或管理员 |
| 新增用户 | POST | `/api/users` | 管理员 |
| 编辑用户 | PUT | `/api/users/{id}` | 管理员 |
| 启停用户 | PATCH | `/api/users/{id}/status` | 管理员 |
| 角色列表 | GET | `/api/roles` | 管理员 |
| 角色详情 | GET | `/api/roles/{id}` | 管理员 |
| 新增角色 | POST | `/api/roles` | 管理员 |
| 编辑角色 | PUT | `/api/roles/{id}` | 管理员 |
| 删除角色 | DELETE | `/api/roles/{id}` | 管理员 |
| 分配角色 | POST | `/api/roles/assign` | 管理员 |
| 黑名单列表 | GET | `/api/blacklist` | 管理员 |
| 黑名单详情 | GET | `/api/blacklist/{id}` | 管理员 |
| 加入黑名单 | POST | `/api/blacklist` | 管理员 |
| 解除黑名单 | PATCH | `/api/blacklist/{id}/release` | 管理员 |
| 查询用户黑名单状态 | GET | `/api/blacklist/status/{userId}` | 管理员或志愿者 |
| 批量解除黑名单 | PATCH | `/api/blacklist/release/batch` | 管理员 |

## 3. 用户登录与用户信息

### 请求：用户登录

| 接口说明 | 使用用户名和密码登录并返回 JWT |
|---|---|
| HTTP URL | `http://localhost:5047/api/auth/login` |
| HTTP Method | `POST` |
| 权限要求 | 公开 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `username` | string | 是 | 登录用户名 |
| `password` | string | 是 | 登录密码 |

```json
{
  "username": "a_group_admin",
  "password": "Passw0rd!"
}
```

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 登录成功 | 用户资料、角色信息和 `token` |
| 400 | 用户名或密码为空 | `{"message":"Username 与 Password 均为必填。"}` |
| 401 | 用户名或密码错误 | `{"message":"用户名或密码错误。"}` |
| 403 | 用户已停用 | `{"message":"当前账号已停用，无法登录。"}` |

### 请求：获取当前登录用户

| 接口说明 | 根据 JWT 获取当前用户及其角色 |
|---|---|
| HTTP URL | `http://localhost:5047/api/auth/me` |
| HTTP Method | `GET` |
| 权限要求 | 已登录 |

#### 请求参数

无 URL 参数；请求头必须携带 `Authorization`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `UserProfileResponse` |
| 401 | Token 无效或缺失 | 未授权 |
| 404 | 用户不存在 | 当前登录用户不存在 |

### 请求：校验当前登录状态

| 接口说明 | 校验 JWT、用户状态和数据库中的当前角色 |
|---|---|
| HTTP URL | `http://localhost:5047/api/auth/check` |
| HTTP Method | `GET` |
| 权限要求 | 已登录 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | Token 有效且用户仍为 ACTIVE | `{"authenticated":true,"user":{...}}` |
| 401 | 未登录、用户停用或角色已变化 | `{"authenticated":false,...}` |

### 请求：退出登录

| 接口说明 | 返回退出响应；JWT 为无状态令牌，客户端需清除本地 Token |
|---|---|
| HTTP URL | `http://localhost:5047/api/auth/logout` |
| HTTP Method | `POST` |
| 权限要求 | 已登录 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 退出响应成功 | message |
| 401 | Token 无效或缺失 | 未授权 |

### 请求：查询用户列表

| 接口说明 | 按用户名、状态或角色筛选用户 |
|---|---|
| HTTP URL | `http://localhost:5047/api/users?username={username}&status={status}&roleId={roleId}` |
| HTTP Method | `GET` |
| 权限要求 | 管理员 |

#### 请求参数

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `username` | string | 否 | 用户名模糊匹配 |
| `status` | string | 否 | `ACTIVE` 或 `DISABLED` |
| `roleId` | string | 否 | 角色 ID |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `UserProfileResponse[]` |
| 400 | 状态值非法 | status 仅支持 ACTIVE 或 DISABLED |
| 401/403 | 未授权或无管理员权限 | 错误信息 |

### 请求：根据 ID 获取用户

| 接口说明 | 管理员可查询任意用户，普通用户只能查询本人 |
|---|---|
| HTTP URL | `http://localhost:5047/api/users/{id}` |
| HTTP Method | `GET` |
| 权限要求 | 本人或管理员 |

#### 请求参数

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `id` | string | 是 | 用户 ID |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `UserProfileResponse` |
| 400 | ID 为空 | 错误信息 |
| 401/403 | 未授权或无权查看 | 错误信息 |
| 404 | 用户不存在 | 错误信息 |

### 请求：新增用户

| 接口说明 | 管理员创建用户；UserID 和 PasswordHash 由服务端生成 |
|---|---|
| HTTP URL | `http://localhost:5047/api/users` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `roleID` | string | 是 | 已存在的角色 ID |
| `username` | string | 是 | 全局唯一用户名 |
| `password` | string | 是 | 初始密码 |
| `realName` | string | 否 | 真实姓名 |
| `studentNo` | string | 否 | 学号 |
| `phone` | string | 否 | 手机号 |
| `verifyStatus` | string | 否 | `VERIFIED` 或 `UNVERIFIED` |
| `status` | string | 否 | `ACTIVE` 或 `DISABLED`，默认 `ACTIVE` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | 新建的 `UserProfileResponse` |
| 400 | 参数或角色非法 | 错误信息 |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 409 | 用户名已存在 | 错误信息 |

### 请求：编辑用户基础信息

| 接口说明 | 修改用户资料、角色和状态；不允许修改用户名和密码 |
|---|---|
| HTTP URL | `http://localhost:5047/api/users/{id}` |
| HTTP Method | `PUT` |
| 权限要求 | 管理员 |

#### 请求体

字段与新增用户相同，但 `username`、`password`、`userID` 不接受修改；`roleID` 必填。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无 |
| 400 | 参数或角色非法 | 错误信息 |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 404 | 用户不存在 | 错误信息 |
| 409 | 更新未生效 | 错误信息 |

### 请求：启用或停用用户

| 接口说明 | 修改用户 `ACTIVE/DISABLED` 状态 |
|---|---|
| HTTP URL | `http://localhost:5047/api/users/{id}/status` |
| HTTP Method | `PATCH` |
| 权限要求 | 管理员 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `status` | string | 是 | `ACTIVE` 或 `DISABLED` |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无 |
| 400 | 状态非法 | 错误信息 |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 404 | 用户不存在 | 错误信息 |
| 409 | 更新未生效 | 错误信息 |

## 4. 角色权限管理

### 请求：查询角色列表 / 查询角色详情

| 接口说明 | 查询角色列表或单个角色 |
|---|---|
| HTTP URL | `http://localhost:5047/api/roles`；`http://localhost:5047/api/roles/{id}` |
| HTTP Method | `GET` |
| 权限要求 | 管理员 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `Role[]` 或 `Role` |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 404 | 角色不存在 | 错误信息 |

### 请求：新增角色

| 接口说明 | 创建角色 |
|---|---|
| HTTP URL | `http://localhost:5047/api/roles` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `roleID` | string | 是 | 角色 ID；当前接口不会自动生成 |
| `roleName` | string | 是 | 角色名 |
| `permissionScope` | string | 否 | 逗号分隔的权限范围 |
| `description` | string | 否 | 角色说明 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 创建成功 | 角色对象 |
| 400/401/403 | 参数或权限错误 | 错误信息 |
| 409 | 角色名冲突 | 错误信息 |

### 请求：编辑或删除角色

| 接口说明 | 修改角色或删除角色；正在被用户使用的角色不能删除 |
|---|---|
| HTTP URL | `http://localhost:5047/api/roles/{id}` |
| HTTP Method | `PUT` 或 `DELETE` |
| 权限要求 | 管理员 |

#### 请求体（PUT）

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `roleName` | string | 是 | 角色名 |
| `permissionScope` | string | 否 | 权限范围 |
| `description` | string | 否 | 角色说明 |
| `roleID` | string | 是 | 必须与 URL 中的 ID 一致 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 修改或删除成功 | 无 |
| 400/401/403 | 参数或权限错误 | 错误信息 |
| 404 | 角色不存在 | 错误信息 |
| 409 | 角色正在使用或更新未生效 | 错误信息 |

### 请求：给用户分配角色

| 接口说明 | 更新用户角色并写入审计信息 |
|---|---|
| HTTP URL | `http://localhost:5047/api/roles/assign` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `userId` | string | 是 | 用户 ID |
| `roleId` | string | 是 | 角色 ID |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 分配成功 | 无 |
| 400 | 参数为空 | 错误信息 |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 404 | 用户或角色不存在 | 错误信息 |
| 409 | 分配未生效 | 错误信息 |

## 5. 用户黑名单管理

### 请求：查询黑名单列表

| 接口说明 | 分页查询黑名单，可按用户、状态和关键词筛选 |
|---|---|
| HTTP URL | `http://localhost:5047/api/blacklist?userId={userId}&status={status}&keyword={keyword}&page=1&pageSize=20` |
| HTTP Method | `GET` |
| 权限要求 | 管理员 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `{items,totalCount,page,pageSize,totalPages}` |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 500 | 数据库查询失败 | 错误信息 |

### 请求：查询黑名单详情

| 接口说明 | 查询一条黑名单记录及操作人名称 |
|---|---|
| HTTP URL | `http://localhost:5047/api/blacklist/{id}` |
| HTTP Method | `GET` |
| 权限要求 | 管理员 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `BlacklistResponseDto` |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 404 | 记录不存在 | 错误信息 |

### 请求：加入黑名单

| 接口说明 | 将用户加入有效黑名单，保留创建人和原因 |
|---|---|
| HTTP URL | `http://localhost:5047/api/blacklist` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `userId` | string | 是 | 被拉黑用户 ID |
| `reasonType` | string | 是 | 原因类型 |
| `reasonDetail` | string | 是 | 原因说明 |
| `applicationId` | string | 否 | 关联领养申请 ID |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 加入成功 | 黑名单 ID 和用户 ID |
| 400 | 请求数据非法 | 错误信息 |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 404 | 用户或关联申请不存在 | 错误信息 |
| 409 | 用户已有有效黑名单 | 错误信息 |

### 请求：解除黑名单

| 接口说明 | 将黑名单状态改为 `RELEASED`，保留历史记录 |
|---|---|
| HTTP URL | `http://localhost:5047/api/blacklist/{id}/release` |
| HTTP Method | `PATCH` |
| 权限要求 | 管理员 |

#### 请求体

可选字段：`releaseReason`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 解除成功 | `blacklistId`、`releasedBy`、`releaseTime` |
| 401/403 | 未授权或无管理员权限 | 错误信息 |
| 404 | 记录不存在 | 错误信息 |
| 409 | 已经解除 | 错误信息 |

### 请求：查询用户黑名单状态

| 接口说明 | 供领养审核查看用户是否存在有效黑名单 |
|---|---|
| HTTP URL | `http://localhost:5047/api/blacklist/status/{userId}` |
| HTTP Method | `GET` |
| 权限要求 | 管理员或志愿者（不是公开接口） |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `{userId,isBlacklisted,blacklistId,reasonType,reasonDetail,blacklistedAt}` |
| 400 | 用户 ID 为空 | 错误信息 |
| 401/403 | 未授权或角色不足 | 错误信息 |
| 404 | 用户不存在 | 错误信息 |

### 请求：批量解除黑名单

| 接口说明 | 批量解除多条黑名单记录 |
|---|---|
| HTTP URL | `http://localhost:5047/api/blacklist/release/batch` |
| HTTP Method | `PATCH` |
| 权限要求 | 管理员 |

#### 请求体

```json
{
  "blacklistIds": ["blacklist-001", "blacklist-002"]
}
```

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 批量处理完成 | `{success:[...],failed:[...]}` |
| 400 | ID 列表为空 | 错误信息 |
| 401/403 | 未授权或无管理员权限 | 错误信息 |

## 6. 本地联调

1. 配置 Oracle 连接串和 `Auth__JwtSecret`。
2. 执行 `database/setup_all.sql`，或按数据库 README 的顺序初始化对象。
3. 使用演示账号 `Passw0rd!` 登录获取 Token。
4. 按本文接口顺序测试：登录 → 用户/角色 → 黑名单。
5. 黑名单状态查询应使用管理员或志愿者 Token；不能按“公开接口”测试。
