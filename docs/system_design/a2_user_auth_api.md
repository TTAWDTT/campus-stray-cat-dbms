\# A组成员2：角色权限与用户黑名单管理 API 说明

涉及文件：\`RolesController.cs\`、\`UserBlacklistController.cs\`

基础地址：\`http://localhost:5047\`

认证方式：除标注"公开"的接口外，需在 Header 携带 \`Authorization: Bearer {token}\`

\## 模块范围

成员2负责角色权限管理与用户黑名单管理。

\- 角色权限管理：角色列表、详情、新增、修改、删除（软删除）、获取启用角色（公开）、分配角色

\- 用户黑名单管理：黑名单列表（分页+筛选）、详情、加入、解除、批量解除、查询用户状态（公开）

\### 数据表

\- \`SYS_ROLES\`

\- \`USER_BLACKLIST\`

\- \`SYS_USERS\`（关联查询）

\- \`LOG_AUDITTRAILS\`（审计日志）

\### 状态契约

\- 黑名单状态：\`Active\` | \`Released\`

\- 角色启用状态：\`1\`（启用）| \`0\`（停用）

\## 一、角色管理接口

\### 1.1 获取角色列表

\*\*接口说明\*\*

获取系统中所有角色（仅管理员）。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/roles\` |

| HTTP Method | \`GET\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| Authorization | string | 是 | 请求头，格式：\`Bearer {token}\` |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 查询成功 | 角色数组 |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

\*\*200 响应示例：\*\*

\`\`\`json

\[

{

"roleId": "role-admin-a-group",

"roleName": "ADMIN",

"description": "系统管理员",

"permissionScope": "USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1",

"userCount": 1

},

{

"roleId": "role-volunteer-a-group",

"roleName": "VOLUNTEER",

"description": "校园志愿者",

"permissionScope": "CAT_VIEW,SIGHTING_WRITE,SHIFT_CHECKIN",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1",

"userCount": 0

},

{

"roleId": "role-user-a-group",

"roleName": "USER",

"description": "普通用户",

"permissionScope": "CAT_VIEW,ADOPT_APPLY",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1",

"userCount": 2

}

\]

\`\`\`

\### 1.2 获取启用角色列表

\*\*接口说明\*\*

获取所有 \`IsActive = 1\` 的角色（公开接口，供前端下拉选择使用，无需登录）。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/roles/active\` |

| HTTP Method | \`GET\` |

| 权限要求 | 无（公开） |

\*\*请求参数\*\*：无

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 查询成功 | 角色数组 |

\*\*200 响应示例：\*\*

\`\`\`json

\[

{

"roleId": "role-admin-a-group",

"roleName": "ADMIN",

"description": "系统管理员",

"permissionScope": "USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1"

},

{

"roleId": "role-volunteer-a-group",

"roleName": "VOLUNTEER",

"description": "校园志愿者",

"permissionScope": "CAT_VIEW,SIGHTING_WRITE,SHIFT_CHECKIN",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1"

},

{

"roleId": "role-user-a-group",

"roleName": "USER",

"description": "普通用户",

"permissionScope": "CAT_VIEW,ADOPT_APPLY",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1"

}

\]

\`\`\`

\### 1.3 获取角色详情

\*\*接口说明\*\*

根据角色 ID 获取详细信息（仅管理员）。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/roles/{id}\` |

| HTTP Method | \`GET\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| Authorization | string | 是 | 请求头，格式：\`Bearer {token}\` |

| id | string | 是 | 路径参数，角色 ID |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 查询成功 | 角色对象 |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 404 | 角色不存在 | \`{"message":"角色不存在"}\` |

\*\*200 响应示例：\*\*

\`\`\`json

{

"roleId": "role-admin-a-group",

"roleName": "ADMIN",

"description": "系统管理员",

"permissionScope": "USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1",

"userCount": 1

}

\`\`\`

\### 1.4 新增角色

\*\*接口说明\*\*

创建新角色（仅管理员）。\`RoleID\` 由服务端生成，需校验角色名称唯一性。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/roles\` |

| HTTP Method | \`POST\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求体\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| roleName | string | 是 | 角色名称，全局唯一 |

| description | string | 否 | 角色描述 |

| permissionScope | string | 否 | 权限范围，多个用逗号分隔 |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 201 | 创建成功 | 新建角色对象 |

| 400 | 参数不合法 | \`{"message":"角色名称不能为空"}\` |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 409 | 角色名称已存在 | \`{"message":"角色名称 'NEW_ROLE' 已存在"}\` |

\*\*201 响应示例：\*\*

\`\`\`json

{

"roleId": "new-role-id-xxx",

"roleName": "NEW_ROLE",

"description": "新角色描述",

"permissionScope": "PERM1,PERM2",

"createdAt": "2026-08-04T10:00:00",

"isActive": "1"

}

\`\`\`

\### 1.5 修改角色

\*\*接口说明\*\*

更新角色信息（仅管理员）。不允许修改 \`RoleID\`。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/roles/{id}\` |

| HTTP Method | \`PUT\` |

| 权限要求 | 管理员（ADMIN） |

\*\*路径参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| id | string | 是 | 角色 ID |

\*\*请求体\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| roleName | string | 是 | 角色名称 |

| description | string | 否 | 角色描述 |

| permissionScope | string | 否 | 权限范围 |

| isActive | string | 是 | \`1\`（启用）或 \`0\`（停用） |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 204 | 更新成功 | 无响应体 |

| 400 | 参数不合法 | \`{"message":"参数错误"}\` |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 404 | 角色不存在 | \`{"message":"角色不存在"}\` |

| 409 | 角色名称冲突 | \`{"message":"角色名称 'xxx' 已被其他角色使用"}\` |

\### 1.6 删除角色（软删除）

\*\*接口说明\*\*

软删除角色，将 \`IsActive\` 设为 \`0\`（仅管理员）。若有用户正在使用该角色，返回 409 拒绝删除。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/roles/{id}\` |

| HTTP Method | \`DELETE\` |

| 权限要求 | 管理员（ADMIN） |

\*\*路径参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| id | string | 是 | 角色 ID |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 204 | 删除成功 | 无响应体 |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 404 | 角色不存在 | \`{"message":"角色不存在"}\` |

| 409 | 角色正在使用 | \`{"message":"该角色有 5 个用户正在使用，无法删除"}\` |

\### 1.7 给用户分配角色

\*\*接口说明\*\*

给指定用户分配新角色（仅管理员）。更新 \`SYS_USERS.ROLEID\`，并写入审计日志。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/roles/assign\` |

| HTTP Method | \`PUT\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求体\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| userId | string | 是 | 用户 ID |

| roleId | string | 是 | 角色 ID |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 分配成功 | 成功信息 |

| 400 | 参数不合法 | \`{"message":"userId 和 roleId 均不能为空"}\` |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 404 | 用户或角色不存在 | \`{"message":"用户不存在"}\` 或 \`{"message":"角色不存在"}\` |

| 409 | 用户已有该角色 | \`{"message":"用户已拥有该角色"}\` |

\*\*200 响应示例：\*\*

\`\`\`json

{

"message": "用户 'a_group_user' 已分配角色 'VOLUNTEER'",

"userId": "user-normal-a-group",

"roleId": "role-volunteer-a-group"

}

\`\`\`

\## 二、黑名单管理接口

\### 2.1 获取黑名单列表

\*\*接口说明\*\*

分页获取黑名单列表，支持按用户 ID、状态筛选（仅管理员）。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/blacklist\` |

| HTTP Method | \`GET\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| Authorization | string | 是 | 请求头，格式：\`Bearer {token}\` |

| userId | string | 否 | 查询参数，按用户 ID 筛选 |

| status | string | 否 | 查询参数，\`Active\` 或 \`Released\` |

| page | int | 否 | 页码，默认 \`1\` |

| pageSize | int | 否 | 每页条数，默认 \`20\`，最大 \`100\` |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 查询成功 | 分页对象 |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

\*\*200 响应示例：\*\*

\`\`\`json

{

"items": \[

{

"blacklistId": "bl-001-a-group",

"userId": "user-normal-a-group",

"userName": "a_group_user",

"reasonType": "违规领养",

"reasonDetail": "多次领养后弃养猫咪，造成猫咪身心伤害",

"applicationId": null,

"createdBy": "user-admin-a-group",

"createdByName": "a_group_admin",

"createdAt": "2026-08-04T10:00:00",

"status": "Active",

"releaseTime": null,

"releasedBy": null,

"releasedByName": null

}

\],

"totalCount": 3,

"page": 1,

"pageSize": 20,

"totalPages": 1

}

\`\`\`

\### 2.2 获取黑名单详情

\*\*接口说明\*\*

根据黑名单记录 ID 获取详情（仅管理员）。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/blacklist/{id}\` |

| HTTP Method | \`GET\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| Authorization | string | 是 | 请求头，格式：\`Bearer {token}\` |

| id | string | 是 | 路径参数，黑名单记录 ID |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 查询成功 | 黑名单记录对象 |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 404 | 记录不存在 | \`{"message":"黑名单记录不存在"}\` |

\*\*200 响应示例：\*\*

\`\`\`json

{

"blacklistId": "bl-001-a-group",

"userId": "user-normal-a-group",

"userName": "a_group_user",

"reasonType": "违规领养",

"reasonDetail": "多次领养后弃养猫咪，造成猫咪身心伤害",

"applicationId": null,

"createdBy": "user-admin-a-group",

"createdByName": "a_group_admin",

"createdAt": "2026-08-04T10:00:00",

"status": "Active",

"releaseTime": null,

"releasedBy": null,

"releasedByName": null

}

\`\`\`

\### 2.3 加入黑名单

\*\*接口说明\*\*

将用户加入黑名单（仅管理员）。若用户已有有效黑名单记录，返回 409 拒绝。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/blacklist\` |

| HTTP Method | \`POST\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求体\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| userId | string | 是 | 被拉黑用户 ID |

| reasonType | string | 是 | 拉黑原因类型 |

| reasonDetail | string | 是 | 详细原因说明 |

| applicationId | string | 否 | 关联的领养申请 ID |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 201 | 加入成功 | 成功信息 |

| 400 | 参数不合法 | \`{"message":"userId 不能为空"}\` |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 404 | 用户不存在 | \`{"message":"用户不存在"}\` |

| 409 | 已在黑名单中 | \`{"message":"该用户已在黑名单中，请勿重复拉黑"}\` |

\*\*201 响应示例：\*\*

\`\`\`json

{

"message": "用户 'a_group_user' 已加入黑名单",

"userId": "user-normal-a-group"

}

\`\`\`

\### 2.4 解除黑名单

\*\*接口说明\*\*

解除黑名单记录（仅管理员）。保留历史记录，状态变更为 \`Released\`。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/blacklist/{id}/release\` |

| HTTP Method | \`PATCH\` |

| 权限要求 | 管理员（ADMIN） |

\*\*路径参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| id | string | 是 | 黑名单记录 ID |

\*\*请求体\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| releaseReason | string | 否 | 解除原因说明 |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 解除成功 | 解除信息 |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

| 404 | 记录不存在 | \`{"message":"黑名单记录不存在"}\` |

| 409 | 已解除 | \`{"message":"该黑名单记录已被解除"}\` |

\*\*200 响应示例：\*\*

\`\`\`json

{

"message": "黑名单已解除",

"blacklistId": "bl-001-a-group",

"releasedBy": "user-admin-a-group",

"releaseTime": "2026-08-04T10:00:00"

}

\`\`\`

\### 2.5 查询用户黑名单状态（公开）

\*\*接口说明\*\*

查询用户是否在有效黑名单中（供领养审核模块调用，无需登录）。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/blacklist/status/{userId}\` |

| HTTP Method | \`GET\` |

| 权限要求 | 无（公开） |

\*\*路径参数\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| userId | string | 是 | 用户 ID |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 查询成功 | 黑名单状态对象 |

| 404 | 用户不存在 | \`{"message":"用户不存在"}\` |

\*\*200 响应示例（在黑名单中）：\*\*

\`\`\`json

{

"userId": "user-normal-a-group",

"isBlacklisted": true,

"blacklistId": "bl-001-a-group",

"reasonType": "违规领养",

"reasonDetail": "多次领养后弃养猫咪",

"blacklistedAt": "2026-08-04T10:00:00"

}

\`\`\`

\*\*200 响应示例（不在黑名单中）：\*\*

\`\`\`json

{

"userId": "user-normal-a-group",

"isBlacklisted": false

}

\`\`\`

\### 2.6 批量解除黑名单

\*\*接口说明\*\*

批量解除多条黑名单记录（仅管理员）。

| 项目 | 内容 |

|------|------|

| HTTP URL | \`http://localhost:5047/api/blacklist/release/batch\` |

| HTTP Method | \`PATCH\` |

| 权限要求 | 管理员（ADMIN） |

\*\*请求体\*\*

| 名称 | 类型 | 必填 | 描述 |

|------|------|------|------|

| blacklistIds | string\[\] | 是 | 要解除的黑名单记录 ID 数组 |

\*\*响应体\*\*

| 状态码 | 描述 | 响应体 |

|--------|------|--------|

| 200 | 操作完成 | 批量操作结果 |

| 400 | 参数不合法 | \`{"message":"请提供要解除的黑名单ID列表"}\` |

| 401 | 未授权 | \`{"message":"未授权"}\` |

| 403 | 无权限 | \`{"message":"无权限"}\` |

\*\*200 响应示例：\*\*

\`\`\`json

{

"message": "成功解除 2 条记录",

"success": \[

"bl-001-a-group",

"bl-003-a-group"

\],

"failed": \[

"bl-002-a-group (已解除)"

\]

}

\`\`\`

\## 三、DTO 结构说明

\### RoleResponseDto

| 字段 | 类型 | 说明 |

|------|------|------|

| roleId | string | 角色 ID |

| roleName | string | 角色名称 |

| description | string | 角色描述 |

| permissionScope | string | 权限范围 |

| createdAt | datetime | 创建时间 |

| isActive | string | \`1\` 启用 / \`0\` 停用 |

| userCount | int | 拥有该角色的用户数 |

\### BlacklistResponseDto

| 字段 | 类型 | 说明 |

|------|------|------|

| blacklistId | string | 记录 ID |

| userId | string | 用户 ID |

| userName | string | 用户名 |

| reasonType | string | 拉黑原因类型 |

| reasonDetail | string | 详细原因 |

| applicationId | string | 关联领养申请 ID |

| createdBy | string | 创建人 ID |

| createdByName | string | 创建人姓名 |

| createdAt | datetime | 创建时间 |

| status | string | \`Active\` / \`Released\` |

| releaseTime | datetime | 解除时间 |

| releasedBy | string | 解除人 ID |

| releasedByName | string | 解除人姓名 |

\### BlacklistStatusDto

| 字段 | 类型 | 说明 |

|------|------|------|

| userId | string | 用户 ID |

| isBlacklisted | bool | 是否在有效黑名单中 |

| blacklistId | string | 黑名单记录 ID |

| reasonType | string | 拉黑原因类型 |

| reasonDetail | string | 详细原因 |

| blacklistedAt | datetime | 拉黑时间 |

\### PagedResult

| 字段 | 类型 | 说明 |

|------|------|------|

| items | array | 数据列表 |

| totalCount | int | 总记录数 |

| page | int | 当前页码 |

| pageSize | int | 每页条数 |

| totalPages | int | 总页数 |

\## 四、测试账号

| 用户名 | 密码 | 角色 |

|--------|------|------|

| a_group_admin | Passw0rd! | ADMIN |

| a_group_volunteer | Passw0rd! | VOLUNTEER |

| a_group_user | Passw0rd! | USER |

\## 五、本地联调

1\. 配置 \`appsettings.Development.json\` 中的 Oracle 连接串（该文件已 gitignore）。

2\. 执行 \`database/create_tables.sql\`。

3\. 执行 \`database/queries/a_group_advanced.sql\` 与 \`database/queries/a_group_demo_data.sql\`。

4\. 运行：\`dotnet run --project src/CampusStrayCatSystem.Core/CampusStrayCatSystem.Core.csproj --launch-profile http\`

5\. 访问 \`http://localhost:5047/swagger\`，或按本文接口调用。

6\. 演示账号密码：\`Passw0rd!\`（见 demo SQL）。

\## 保存方法

1\. 复制上面的全部内容

2\. 在项目 \`docs/\` 目录下新建文件，命名为 \`a2_role_blacklist_api.md\`

3\. 粘贴保存即可