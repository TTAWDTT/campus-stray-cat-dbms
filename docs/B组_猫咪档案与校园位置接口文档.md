# B 组（功能点 5-10）：猫咪档案与校园位置接口测试文档

## 1. 公共约定

| 项目 | 约定 |
|---|---|
| 基础地址 | `http://localhost:5047` |
| 认证方式 | 受保护接口携带 `Authorization: Bearer <token>`；猫咪/区域/目击/照片查询接口当前允许公开访问 |
| 数据格式 | JSON；照片上传使用 `multipart/form-data` |
| 时间格式 | ISO 8601 |
| 猫咪生活状态 | `ON_CAMPUS`、`MISSING`、`ADOPTED`、`DECEASED` |
| 猫咪档案状态 | `DRAFT`、`PUBLISHED`、`ARCHIVED` |
| 命名投票 | 候选名最多 50 个 UTF-8 字节；同一用户不可重复投票 |

校园区域的正式接口是 `/api/campus-areas`；代码中遗留的 `/api/areas` 仅提供旧版列表/详情查询，不作为本组测试入口。

## 2. 接口总览

| 功能 | 方法 | URL | 权限 |
|---|---|---|---|
| 猫咪列表/详情 | GET | `/api/cats`、`/api/cats/{catId}` | 公开 |
| 新增/编辑/归档猫咪 | POST/PUT/DELETE | `/api/cats`、`/api/cats/{catId}` | 管理员或志愿者 |
| 区域 CRUD | GET/POST/PUT/DELETE | `/api/campus-areas...` | 查询公开；写操作管理员或志愿者 |
| 服务点 CRUD | GET/POST/PUT/DELETE | `/api/service-points...` | 管理员或志愿者 |
| 猫窝维护 CRUD | GET/POST/PUT/DELETE | `/api/nest-maintenance-records...` | 管理员或志愿者 |
| 目击记录 CRUD | GET/POST/PUT/DELETE | `/api/cat-sightings...` | 查询公开；写操作按接口说明 |
| 照片与特征 | GET/POST/PUT/DELETE | `/api/cats/{catId}/photos...` | 查询公开；上传/维护需管理员或志愿者 |
| 命名候选与投票 | GET/POST | `/api/naming-votes...` | 发布/定名需管理员或志愿者；投票需登录 |

## 3. 猫咪档案

### 请求：查询猫咪列表

| 接口说明 | 按主要区域、生活状态和档案状态筛选猫咪 |
|---|---|
| HTTP URL | `http://localhost:5047/api/cats?mainAreaId={id}&lifeStatus={status}&archiveStatus={status}` |
| HTTP Method | `GET` |
| 权限要求 | 公开 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `CatSummary[]` |
| 400 | 状态值非法 | 错误信息 |

### 请求：查询猫咪详情

| 接口说明 | 查询单只猫咪档案 |
|---|---|
| HTTP URL | `http://localhost:5047/api/cats/{catId}` |
| HTTP Method | `GET` |
| 权限要求 | 公开 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | 猫咪档案对象 |
| 404 | 猫咪不存在 | 错误信息 |

### 请求：新增猫咪档案

| 接口说明 | 创建猫咪档案，档案状态默认 `DRAFT` |
|---|---|
| HTTP URL | `http://localhost:5047/api/cats` |
| HTTP Method | `POST` |
| 权限要求 | 管理员或志愿者 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `catName` | string | 否 | 昵称 |
| `gender` | string | 否 | 性别 |
| `breed` | string | 否 | 品种 |
| `colorPattern` | string | 否 | 花色 |
| `sterilizedFlag` | int | 否 | 是否绝育，0/1 |
| `earTipFlag` | int | 否 | 是否剪耳，0/1 |
| `personalityTags` | string | 否 | 性格标签 |
| `mainAreaId` | string | 否 | 主要活动区域 ID |
| `lifeStatus` | string | 否 | 生活状态 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | `CatSummary` |
| 400 | 区域不存在或参数非法 | 错误信息 |
| 401/403 | 未授权或角色不足 | 错误信息 |

### 请求：编辑或归档猫咪

| 接口说明 | 更新档案，DELETE 实际执行归档而非物理删除 |
|---|---|
| HTTP URL | `http://localhost:5047/api/cats/{catId}` |
| HTTP Method | `PUT` 或 `DELETE` |
| 权限要求 | 管理员或志愿者 |

#### 请求体（PUT）

字段与新增猫咪相同；`archiveStatus` 可为 `DRAFT`、`PUBLISHED`、`ARCHIVED`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 更新或归档成功 | 无 |
| 400 | 区域或状态非法 | 错误信息 |
| 401/403 | 未授权或角色不足 | 错误信息 |
| 404 | 猫咪不存在 | 错误信息 |

## 4. 校园区域、服务点与猫窝维护

### 请求：查询校园区域

| 接口说明 | 支持条件查询、根区域、层级和子区域查询 |
|---|---|
| HTTP URL | `http://localhost:5047/api/campus-areas`、`/roots`、`/hierarchy`、`/{id}`、`/{id}/children` |
| HTTP Method | `GET` |
| 权限要求 | 查询公开；写操作管理员或志愿者 |

#### 请求参数

`campusName`、`areaType`、`riskLevel` 为条件查询参数；`id` 为路径参数。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `CampusArea[]` 或区域对象 |
| 404 | 区域不存在 | 错误信息 |

### 请求：新增、更新或删除校园区域

| 接口说明 | 维护校园区域；删除前不能存在下级区域 |
|---|---|
| HTTP URL | `http://localhost:5047/api/campus-areas` 或 `/api/campus-areas/{id}` |
| HTTP Method | `POST`、`PUT`、`DELETE` |
| 权限要求 | 管理员或志愿者 |

#### 请求体（POST/PUT）

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `areaName` | string | 是 | 区域名称 |
| `campusName` | string | 否 | 校区名称 |
| `parentAreaID` | string | 否 | 父区域 ID |
| `areaType` | string | 否 | 区域类型 |
| `riskLevel` | string | 否 | 风险等级 |
| `geoBoundary` | string | 否 | 地理边界 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 新增成功 | 区域对象 |
| 204 | 更新或删除成功 | 无 |
| 400 | 参数或父区域非法 | 错误信息 |
| 404 | 区域不存在 | 错误信息 |
| 409 | 存在下级区域或数据冲突 | 错误信息 |

### 请求：查询、新增、更新或删除服务点

| 接口说明 | 维护投喂点、猫窝等服务点 |
|---|---|
| HTTP URL | `http://localhost:5047/api/service-points` 或 `/api/service-points/{id}` |
| HTTP Method | `GET`、`POST`、`PUT`、`DELETE` |
| 权限要求 | 管理员或志愿者 |

#### 请求参数/请求体

查询支持 `areaId`、`pointType`、`facilityStatus`；写入字段：`areaID`、`pointName`、`pointType`、`longitude`、`latitude`、`facilityStatus`、`deployTime`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `ServicePoint[]` 或对象 |
| 201 | 新增成功 | 服务点对象 |
| 204 | 更新或删除成功 | 无 |
| 404 | 点位不存在 | 错误信息 |

### 请求：查询、新增、更新或删除猫窝维护记录

| 接口说明 | 记录猫窝检查、损坏程度、维护动作和下次检查时间 |
|---|---|
| HTTP URL | `http://localhost:5047/api/nest-maintenance-records` 或 `/api/nest-maintenance-records/{id}` |
| HTTP Method | `GET`、`POST`、`PUT`、`DELETE` |
| 权限要求 | 管理员或志愿者 |

#### 请求参数/请求体

查询支持 `pointId`、`damageLevel`、`from`、`to`；写入字段：`pointID`、`materialType`、`checkTime`、`weatherCondition`、`damageLevel`、`actionType`、`operatorUserID`、`nextCheckTime`、`remark`。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `NestMaintenanceRecord[]` 或对象 |
| 201 | 新增成功 | 维护记录对象 |
| 204 | 更新或删除成功 | 无 |
| 404 | 记录或点位不存在 | 错误信息 |

## 5. 目击记录

### 请求：查询目击记录

| 接口说明 | 按猫咪、区域和时间范围查询目击记录，或查询最近记录 |
|---|---|
| HTTP URL | `http://localhost:5047/api/cat-sightings`；`/api/cat-sightings/{id}`；`/api/cat-sightings/recent/by-cat/{catId}?limit=10` |
| HTTP Method | `GET` |
| 权限要求 | 公开 |

#### 请求参数

条件查询支持 `catId`、`areaId`、`from`、`to`；最近记录的 `limit` 为 1—100。

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `CatSighting[]` 或单条记录 |
| 400 | 查询参数非法 | 错误信息 |
| 404 | 记录或猫咪不存在 | 错误信息 |

### 请求：新增、更新或删除目击记录

| 接口说明 | 记录猫咪出现地点和时间；更新时保留原始 UserID |
|---|---|
| HTTP URL | `http://localhost:5047/api/cat-sightings` 或 `/api/cat-sightings/{id}` |
| HTTP Method | `POST`、`PUT`、`DELETE` |
| 权限要求 | 新增需登录；修改/删除需管理员或志愿者；用户身份从 JWT 获取 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `catID` | string | 是 | 猫咪 ID |
| `areaID` | string | 是 | 区域 ID |
| `longitude` | decimal | 否 | 经度 |
| `latitude` | decimal | 否 | 纬度 |
| `photoUrl` | string | 否 | 照片 URL |
| `sightingTime` | DateTime | 否 | 目击时间 |
| `remark` | string | 否 | 备注 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 新增成功 | 目击记录对象 |
| 204 | 更新或删除成功 | 无 |
| 400 | 猫咪、区域或时间非法 | 错误信息 |
| 404 | 关联对象不存在 | 错误信息 |

## 6. 猫咪照片与识别特征

### 请求：查询照片、单张照片或识别特征

| 接口说明 | 查询某猫的照片列表、单张照片或特征向量 |
|---|---|
| HTTP URL | `/api/cats/{catId}/photos`、`/api/cats/{catId}/photos/{photoId}`、`/api/cats/{catId}/photos/{photoId}/feature` |
| HTTP Method | `GET` |
| 权限要求 | 公开 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `CatPhoto[]`、`CatPhoto` 或 `{photoID,catID,featureVector}` |
| 404 | 猫咪或照片不存在 | 错误信息 |
| 500 | 特征 JSON 无法解析 | 错误信息 |

### 请求：上传照片

| 接口说明 | 上传 JPEG/PNG 照片；上传人从 JWT 获取 |
|---|---|
| HTTP URL | `http://localhost:5047/api/cats/{catId}/photos` |
| HTTP Method | `POST` |
| 权限要求 | 管理员或志愿者 |
| Content-Type | `multipart/form-data` |

#### 请求参数

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `file` | file | 是 | JPEG/PNG，最大 10 MiB |
| `isPrimary` | int | 否 | `0` 或 `1` |
| `uploadUserID` | string | 否 | 忽略客户端值，以 JWT 用户为准 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 上传成功 | 照片元数据 |
| 400 | 文件格式、大小或字段非法 | 错误信息 |
| 404 | 猫咪不存在 | 错误信息 |
| 409 | 猫咪已归档 | 错误信息 |

### 请求：设置主图或删除照片

| 接口说明 | 设置唯一主图，或删除照片并自动选择新的主图 |
|---|---|
| HTTP URL | `/api/cats/{catId}/photos/{photoId}/primary`；`/api/cats/{catId}/photos/{photoId}` |
| HTTP Method | `PUT` 或 `DELETE` |
| 权限要求 | 管理员或志愿者 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 204 | 操作成功 | 无 |
| 404 | 照片不存在或不属于该猫 | 错误信息 |
| 409 | 照片仍被匹配记录引用或猫咪已归档 | 错误信息 |

## 7. 猫咪命名投票

### 请求：查询候选名

| 接口说明 | 查询某只猫的命名候选及票数 |
|---|---|
| HTTP URL | `http://localhost:5047/api/naming-votes/cats/{catId}/candidates` |
| HTTP Method | `GET` |
| 权限要求 | 已登录 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 查询成功 | `NamingCandidate[]` |
| 404 | 猫咪不存在 | 错误信息 |

### 请求：发布候选名

| 接口说明 | 发布命名候选；提议人从 JWT 获取 |
|---|---|
| HTTP URL | `http://localhost:5047/api/naming-votes/cats/{catId}/candidates` |
| HTTP Method | `POST` |
| 权限要求 | 管理员或志愿者 |

#### 请求体

| 名称 | 类型 | 必填 | 描述 |
|---|---|---|---|
| `candidateName` | string | 是 | 最多 50 个 UTF-8 字节 |
| `deadline` | DateTime | 否 | 必须晚于当前时间 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 201 | 创建成功 | 候选名对象 |
| 400 | 名称或截止时间非法 | 错误信息 |
| 404 | 猫咪不存在 | 错误信息 |
| 409 | 猫咪已归档 | 错误信息 |

### 请求：投票

| 接口说明 | 当前登录用户为候选名投票；同一用户不可重复投票 |
|---|---|
| HTTP URL | `http://localhost:5047/api/naming-votes/candidates/{candidateId}/vote` |
| HTTP Method | `POST` |
| 权限要求 | 已登录 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 投票成功 | message |
| 401 | 未登录 | 未授权 |
| 409 | 候选不存在、已截止、已获胜、猫已归档或重复投票 | 错误信息 |

### 请求：确定获胜名称

| 接口说明 | 截止后由管理员确定唯一最高票候选并更新猫咪昵称 |
|---|---|
| HTTP URL | `http://localhost:5047/api/naming-votes/candidates/{candidateId}/winner` |
| HTTP Method | `POST` |
| 权限要求 | 管理员 |

#### 响应体

| 状态码 | 描述 | 响应体 |
|---|---|---|
| 200 | 确定成功 | message |
| 409 | 未截止、已归档、平票或候选不为最高票 | 错误信息 |

## 8. 本地联调

1. 执行 `database/setup_all.sql`。
2. 使用管理员/志愿者 Token 测试写接口，普通登录用户测试查询和投票。
3. 照片接口可导入 `system_design/B组_猫咪照片Postman集合.json`。
4. 命名投票应依次测试：发布候选 → 多用户投票 → 截止前拒绝定名 → 截止后唯一最高票定名。
