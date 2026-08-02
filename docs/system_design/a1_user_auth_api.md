# A组成员1：用户登录与用户信息管理 API 说明

涉及文件：`AuthController.cs`、`UsersController.cs`  
基础地址：`http://localhost:5047`  
认证方式：除登录外，受保护接口需在 Header 携带 `Authorization: Bearer {token}`  

## 模块范围

成员1负责用户登录与用户信息管理。角色权限管理、黑名单由成员2交付后与本模块一并审查。

状态契约：

- `STATUS`：`ACTIVE` | `DISABLED`
- `VERIFYSTATUS`：`VERIFIED` | `UNVERIFIED`

主键均为 `VARCHAR2(36)` 字符串。`UserID`、`PasswordHash` 由服务端生成；接口响应不返回密码或哈希。

## 接口一览

| 方法 | 路径 | 用途 |
|---|---|---|
| POST | `/api/auth/login` | 登录并返回用户、角色与 JWT |
| GET | `/api/auth/me` | 获取当前登录用户及角色信息 |
| GET | `/api/auth/check` | 校验当前登录状态 |
| POST | `/api/auth/logout` | 退出登录（客户端清除 Token） |
| GET | `/api/users` | 用户列表，支持 username/status/roleId 筛选 |
| GET | `/api/users/{id}` | 查询单个用户 |
| POST | `/api/users` | 新增用户（管理员） |
| PUT | `/api/users/{id}` | 编辑用户基础信息（管理员） |
| PATCH | `/api/users/{id}/status` | 启用或停用用户（管理员） |

---

## 用户登录模块（AuthController）

基础路径：`/api/auth`

### 请求：用户登录

**接口说明**  
使用用户名和密码登录，校验账号存在性、密码正确性与账户状态；成功后返回用户身份、角色信息与 JWT Token。

**HTTP URL**  
`http://localhost:5047/api/auth/login`

**HTTP Method**  
`POST`

**请求体**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| Username | string | 是 | 登录用户名，示例值：`"a_group_admin"` |
| Password | string | 是 | 登录密码明文，示例值：`"Passw0rd!"` |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 登录成功 | 见下方示例 |
| 400 | 参数不合法 | `{"message":"Username 与 Password 均为必填。"}` |
| 401 | 用户名或密码错误 | `{"message":"用户名或密码错误。"}` |
| 403 | 账号已停用 | `{"message":"当前账号已停用，无法登录。"}` |

200 响应示例：

```json
{
  "userID": "user-admin-a-group",
  "username": "a_group_admin",
  "realName": "A组管理员",
  "roleID": "role-admin-a-group",
  "roleName": "ADMIN",
  "permissionScope": "USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE",
  "token": "<JWT>",
  "expiresAtUtc": "2026-08-02T11:52:36.248134Z"
}
```

### 请求：获取当前登录用户

**接口说明**  
根据请求中的 JWT 解析当前用户 ID，返回用户资料与角色信息。

**HTTP URL**  
`http://localhost:5047/api/auth/me`

**HTTP Method**  
`GET`

**请求参数**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| Authorization | string | 是 | 请求头，格式：`Bearer {token}` |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | 用户资料对象（见示例） |
| 401 | 未登录或令牌无效 | `{"message":"登录状态无效，请重新登录。"}` |
| 404 | 用户不存在 | `{"message":"当前登录用户不存在。"}` |

200 响应示例：

```json
{
  "userID": "user-admin-a-group",
  "roleID": "role-admin-a-group",
  "username": "a_group_admin",
  "realName": "A组管理员",
  "studentNo": "A20260001",
  "phone": "13800000001",
  "verifyStatus": "VERIFIED",
  "status": "ACTIVE",
  "roleName": "ADMIN",
  "permissionScope": "USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE"
}
```

### 请求：校验当前登录状态

**接口说明**  
校验 JWT 是否有效，以及对应用户是否仍存在且为 `ACTIVE`。

**HTTP URL**  
`http://localhost:5047/api/auth/check`

**HTTP Method**  
`GET`

**请求参数**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| Authorization | string | 是 | 请求头，格式：`Bearer {token}` |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 已登录 | `{"authenticated":true,"user":{...}}` |
| 401 | 未登录或已失效 | `{"authenticated":false,"message":"当前登录状态已失效。"}` |

### 请求：退出登录

**接口说明**  
服务端返回退出响应。当前实现使用无状态 JWT，不维护服务端吊销列表；客户端需删除本地 Token 才视为退出完成。

**HTTP URL**  
`http://localhost:5047/api/auth/logout`

**HTTP Method**  
`POST`

**请求参数**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| Authorization | string | 是 | 请求头，格式：`Bearer {token}` |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 退出响应成功 | `{"message":"服务端已完成退出响应。JWT 为无状态令牌，如需立即失效请前端清除本地 Token。"}` |
| 401 | 未授权 | 未携带有效 Token |

---

## 用户信息管理模块（UsersController）

基础路径：`/api/users`

本模块全部接口需登录。新增、编辑、启停仅管理员（`RoleName=ADMIN` 或 `PermissionScope` 含 `USER_MANAGE`）可调用；普通用户调用管理接口返回 403。

### 请求：查询用户列表

**接口说明**  
查询用户列表，支持按用户名模糊、状态、角色 ID 筛选。不返回 `PasswordHash`。

**HTTP URL**  
`http://localhost:5047/api/users`

**HTTP Method**  
`GET`

**请求参数**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| Authorization | string | 是 | 请求头 Bearer Token |
| username | string | 否 | 查询参数，用户名模糊匹配 |
| status | string | 否 | 查询参数，仅 `ACTIVE` 或 `DISABLED` |
| roleId | string | 否 | 查询参数，角色 ID |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | 用户资料数组 |
| 400 | 参数不合法 | `{"message":"status 仅支持 ACTIVE 或 DISABLED。"}` |
| 401 | 未授权 | 未登录或 Token 无效 |

### 请求：根据 id 获取单个用户

**接口说明**  
根据用户 ID 查询用户详情与角色信息。

**HTTP URL**  
`http://localhost:5047/api/users/{id}`

**HTTP Method**  
`GET`

**请求参数**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| Authorization | string | 是 | 请求头 Bearer Token |
| id | string | 是 | 路径参数，用户 ID |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | 用户资料对象 |
| 400 | 参数不合法 | `{"message":"用户 ID 不能为空。"}` |
| 401 | 未授权 | 未登录或 Token 无效 |
| 404 | 未找到 | `{"message":"未找到 ID 为 user-xxx 的用户。"}` |

### 请求：新增用户

**接口说明**  
管理员新增用户。`UserID` 与 `PasswordHash` 由服务端生成；需校验用户名唯一、`RoleID` 存在、状态枚举合法。

**HTTP URL**  
`http://localhost:5047/api/users`

**HTTP Method**  
`POST`

**请求体**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| RoleID | string | 是 | 角色 ID，必须已存在于 `SYS_ROLES` |
| Username | string | 是 | 用户名，全局唯一 |
| Password | string | 是 | 初始密码明文，至少 6 位 |
| RealName | string | 否 | 真实姓名 |
| StudentNo | string | 否 | 学号 |
| Phone | string | 否 | 手机号 |
| VerifyStatus | string | 否 | `VERIFIED` / `UNVERIFIED`；缺省 `UNVERIFIED` |
| Status | string | 否 | `ACTIVE` / `DISABLED`；缺省 `ACTIVE` |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | 新建用户资料对象 |
| 400 | 参数/业务校验失败 | 缺必填、RoleID 不存在、枚举非法等 |
| 401 | 未授权 | 未登录或 Token 无效 |
| 403 | 无权限 | 普通用户调用时拒绝 |
| 409 | 用户名冲突 | `{"message":"用户名 xxx 已存在。"}` |

201 响应示例：

```json
{
  "userID": "3cf99a14-64a5-42a0-b015-d259574f51e7",
  "roleID": "role-user-a-group",
  "username": "a_group_newuser",
  "realName": "新用户",
  "studentNo": null,
  "phone": null,
  "verifyStatus": "UNVERIFIED",
  "status": "ACTIVE",
  "roleName": "USER",
  "permissionScope": "CAT_VIEW,ADOPT_APPLY"
}
```

### 请求：编辑用户基础信息

**接口说明**  
管理员编辑用户基础信息。不允许通过本接口修改 `Username`、`PasswordHash`、`UserID`。正式角色分配也可由成员2的分配角色接口承接，联调时需约定。

**HTTP URL**  
`http://localhost:5047/api/users/{id}`

**HTTP Method**  
`PUT`

**请求参数 / 请求体**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| id | string | 是 | 路径参数，用户 ID |
| RoleID | string | 是 | 角色 ID，必须存在 |
| RealName | string | 否 | 真实姓名 |
| StudentNo | string | 否 | 学号 |
| Phone | string | 否 | 手机号 |
| VerifyStatus | string | 否 | `VERIFIED` / `UNVERIFIED` |
| Status | string | 否 | `ACTIVE` / `DISABLED`；不传则保持原状态 |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无响应体 |
| 400 | 参数/业务校验失败 | RoleID 为空或不存在、枚举非法等 |
| 401 | 未授权 | 未登录或 Token 无效 |
| 403 | 无权限 | 普通用户调用时拒绝 |
| 404 | 未找到 | 用户不存在 |
| 409 | 更新未生效 | `{"message":"用户更新未生效，请刷新后重试。"}` |

### 请求：启用或停用用户

**接口说明**  
管理员更新用户账户状态。采用状态变更，不物理删除，以避免破坏其他业务表外键。

**HTTP URL**  
`http://localhost:5047/api/users/{id}/status`

**HTTP Method**  
`PATCH`

**请求参数 / 请求体**

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| id | string | 是 | 路径参数，用户 ID |
| Status | string | 是 | 请求体字段，仅 `ACTIVE` 或 `DISABLED` |

**响应体**

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新成功 | 无响应体 |
| 400 | 参数不合法 | Status 为空或非法 |
| 401 | 未授权 | 未登录或 Token 无效 |
| 403 | 无权限 | 普通用户调用时拒绝 |
| 404 | 未找到 | 用户不存在 |
| 409 | 更新未生效 | 状态更新未生效 |

---

## 本地联调

1. 配置 `appsettings.Development.json` 中的 Oracle 连接串（该文件已 gitignore）。
2. 执行 `database/create_tables.sql`。
3. 执行 `database/queries/a_group_advanced.sql` 与 `database/queries/a_group_demo_data.sql`。
4. 运行：`dotnet run --project src/CampusStrayCatSystem.Core/CampusStrayCatSystem.Core.csproj --launch-profile http`
5. 访问 `http://localhost:5047/swagger`，或按本文接口调用。
6. 演示账号密码：`Passw0rd!`（见 demo SQL）。
7. 若本机开启 HTTP 代理，访问 localhost 时需设置 `NO_PROXY=127.0.0.1,localhost`。
