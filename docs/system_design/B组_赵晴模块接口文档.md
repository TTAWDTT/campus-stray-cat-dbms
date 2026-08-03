# B组 赵晴模块接口文档

| 项目 | 内容 |
|---|---|
| 小组 | B组（徐千顺、赵晴） |
| 本文负责内容 | 赵晴：校园区域、服务点、猫窝维护记录、猫咪目击记录 |
| 文档日期 | 2026-08-03 |
| 对应控制器 | `CampusAreasController`、`ServicePointsController`、`NestMaintenanceRecordsController`、`CatSightingsController` |
| 交叉测试集合 | `assets/demo/ZhaoQing.postman_collection.json` |

## 1. 公共约定

- 本地公共前缀：`http://localhost:5047/api`。
- 下文 URL 均已去除公共前缀。例如 `/campus-areas` 的完整本地地址为 `http://localhost:5047/api/campus-areas`。
- 请求体与成功响应体使用 `application/json`；控制器返回的业务错误为文本字符串。
- JSON 字段采用 camelCase；所有 ID 均为最长 36 字符的字符串，创建时由后端生成 UUID。
- 日期时间使用 ISO 8601，例如 `2026-08-03T09:30:00Z`。
- 查询列表无数据时返回 `200` 和空数组 `[]`，不是 `404`。
- `POST` 成功返回 `201`、新对象和 `Location` 响应头；`PUT`、`DELETE` 成功返回 `204`，响应体为空。
- `PUT` 是完整更新。除 ID 可省略外，应提交该资源需要保留的全部字段；请求体中若带 ID，必须与 URL 中的 ID 相同。

## 2. 接口总览

| 模块 | 方法 | URL（已去除公共前缀） | 说明 |
|---|---|---|---|
| 校园区域 | GET | `/campus-areas` | 条件查询区域 |
| 校园区域 | GET | `/campus-areas/roots` | 查询根区域 |
| 校园区域 | GET | `/campus-areas/hierarchy` | 查询区域层级 |
| 校园区域 | GET | `/campus-areas/{id}` | 按 ID 查询区域 |
| 校园区域 | GET | `/campus-areas/{id}/children` | 查询直接下级区域 |
| 校园区域 | POST | `/campus-areas` | 新增区域 |
| 校园区域 | PUT | `/campus-areas/{id}` | 更新区域 |
| 校园区域 | DELETE | `/campus-areas/{id}` | 删除区域 |
| 服务点 | GET | `/service-points` | 条件查询服务点或猫窝 |
| 服务点 | GET | `/service-points/{id}` | 按 ID 查询服务点 |
| 服务点 | POST | `/service-points` | 新增服务点 |
| 服务点 | PUT | `/service-points/{id}` | 更新服务点 |
| 服务点 | DELETE | `/service-points/{id}` | 删除服务点 |
| 猫窝维护 | GET | `/nest-maintenance-records` | 条件查询维护记录 |
| 猫窝维护 | GET | `/nest-maintenance-records/{id}` | 按 ID 查询维护记录 |
| 猫窝维护 | POST | `/nest-maintenance-records` | 新增维护记录 |
| 猫窝维护 | PUT | `/nest-maintenance-records/{id}` | 更新维护记录 |
| 猫窝维护 | DELETE | `/nest-maintenance-records/{id}` | 删除维护记录 |
| 目击记录 | GET | `/cat-sightings` | 条件查询目击记录 |
| 目击记录 | GET | `/cat-sightings/{id}` | 按 ID 查询目击记录 |
| 目击记录 | GET | `/cat-sightings/recent/by-cat/{catId}` | 查询某只猫的最近目击 |
| 目击记录 | POST | `/cat-sightings` | 新增目击记录 |
| 目击记录 | PUT | `/cat-sightings/{id}` | 更新目击记录 |
| 目击记录 | DELETE | `/cat-sightings/{id}` | 删除目击记录 |

## 3. 数据结构

### 3.1 CampusArea

| 字段 | 类型 | POST 必填 | PUT 必填 | 说明 |
|---|---|---:|---:|---|
| `areaID` | string | 否 | 否 | POST 时忽略并由后端生成；PUT 时如提供，必须与 URL 的 `id` 相同 |
| `areaName` | string | 是 | 是 | 区域名称，去除首尾空白后长度为 1—100 |
| `campusName` | string/null | 否 | 否 | 校区名称 |
| `parentAreaID` | string/null | 否 | 否 | 父区域 ID；必须存在，不能指向自身或形成循环 |
| `areaType` | string/null | 否 | 否 | 区域类型，例如“校区”“公共区域” |
| `riskLevel` | string/null | 否 | 否 | 风险等级，例如“低”“中”“高” |
| `geoBoundary` | string/null | 否 | 否 | 地理边界文本，数据库类型为 CLOB |

层级查询在上述字段之外增加：

| 字段 | 类型 | 说明 |
|---|---|---|
| `hierarchyLevel` | integer | Oracle 层级深度，根区域为 1 |

### 3.2 ServicePoint

| 字段 | 类型 | POST 必填 | PUT 必填 | 说明 |
|---|---|---:|---:|---|
| `pointID` | string | 否 | 否 | 后端生成；PUT 时如提供，必须与 URL 的 `id` 相同 |
| `areaID` | string/null | 否 | 否 | 所属区域 ID；非空时必须存在 |
| `pointName` | string | 是 | 是 | 点位名称，去除首尾空白后长度为 1—100 |
| `pointType` | string/null | 否 | 否 | 点位类型，例如“喂食点”“猫窝” |
| `longitude` | number/null | 否 | 否 | 经度，范围 `-180`—`180`；必须与纬度同时提供或同时为空 |
| `latitude` | number/null | 否 | 否 | 纬度，范围 `-90`—`90`；必须与经度同时提供或同时为空 |
| `facilityStatus` | string/null | 否 | 否 | 设施状态，例如“正常”“需巡查” |
| `deployTime` | datetime/null | 否 | 否 | 部署时间 |

### 3.3 NestMaintenanceRecord

| 字段 | 类型 | POST 必填 | PUT 必填 | 说明 |
|---|---|---:|---:|---|
| `maintenanceID` | string | 否 | 否 | 后端生成；PUT 时如提供，必须与 URL 的 `id` 相同 |
| `pointID` | string | 是 | 是 | 猫窝点位 ID，必须存在 |
| `materialType` | string/null | 否 | 否 | 材料类型，例如“保温箱” |
| `checkTime` | datetime/null | 否 | 否 | 本次巡查时间；POST 未提供时使用服务器当前 UTC 时间 |
| `weatherCondition` | string/null | 否 | 否 | 天气状况 |
| `damageLevel` | string/null | 否 | 否 | 损坏程度，例如“正常”“轻微”“严重” |
| `actionType` | string | 是 | 是 | 维护动作，例如“清理”“维修”“更换” |
| `operatorUserID` | string/null | 否 | 否 | 操作用户 ID；非空时必须存在 |
| `nextCheckTime` | datetime/null | 否 | 否 | 下次巡查时间，不能早于 `checkTime` |
| `remark` | string/null | 否 | 否 | 备注 |

### 3.4 CatSighting

| 字段 | 类型 | POST 必填 | PUT 必填 | 说明 |
|---|---|---:|---:|---|
| `sightingID` | string | 否 | 否 | 后端生成；PUT 时如提供，必须与 URL 的 `id` 相同 |
| `catID` | string | 是 | 是 | 猫咪 ID，必须存在 |
| `userID` | string | 是 | 是 | 上报用户 ID，必须存在 |
| `areaID` | string | 是 | 是 | 目击区域 ID，必须存在 |
| `longitude` | number/null | 否 | 否 | 经度，范围 `-180`—`180`；必须与纬度同时提供或同时为空 |
| `latitude` | number/null | 否 | 否 | 纬度，范围 `-90`—`90`；必须与经度同时提供或同时为空 |
| `photoUrl` | string/null | 否 | 否 | 目击照片 URL |
| `sightingTime` | datetime/null | 否 | 否 | 目击时间；POST 未提供时使用服务器当前 UTC 时间 |
| `remark` | string/null | 否 | 否 | 备注 |

## 4. 校园区域接口

### 4.1 条件查询区域

| 项目 | 内容 |
|---|---|
| 接口说明 | 查询校园区域，支持组合筛选 |
| HTTP Method | `GET` |
| URL | `/campus-areas` |

查询参数：

| 名称 | 类型 | 必填 | 描述 |
|---|---|---:|---|
| `campusName` | string | 否 | 按校区名称精确匹配 |
| `areaType` | string | 否 | 按区域类型精确匹配 |
| `riskLevel` | string | 否 | 按风险等级精确匹配 |

请求示例：

```http
GET /campus-areas?campusName=四平路校区&riskLevel=低
```

响应：

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `CampusArea[]`，无数据时为 `[]` |

```json
[
  {
    "areaID": "demo-area-library",
    "areaName": "图书馆周边",
    "campusName": "四平路校区",
    "parentAreaID": "demo-area-siping",
    "areaType": "公共区域",
    "riskLevel": "低",
    "geoBoundary": null
  }
]
```

### 4.2 查询根区域

| 项目 | 内容 |
|---|---|
| 接口说明 | 查询 `parentAreaID` 为空的根区域 |
| HTTP Method | `GET` |
| URL | `/campus-areas/roots` |

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `CampusArea[]`，无数据时为 `[]` |

### 4.3 查询区域层级

| 项目 | 内容 |
|---|---|
| 接口说明 | 使用 Oracle 层级查询返回所有区域及其层级深度 |
| HTTP Method | `GET` |
| URL | `/campus-areas/hierarchy` |

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `CampusAreaHierarchyItem[]`，无数据时为 `[]` |

```json
[
  {
    "areaID": "demo-area-siping",
    "areaName": "四平路校区",
    "campusName": "四平路校区",
    "parentAreaID": null,
    "areaType": "校区",
    "riskLevel": "低",
    "geoBoundary": null,
    "hierarchyLevel": 1
  },
  {
    "areaID": "demo-area-library",
    "areaName": "图书馆周边",
    "campusName": "四平路校区",
    "parentAreaID": "demo-area-siping",
    "areaType": "公共区域",
    "riskLevel": "低",
    "geoBoundary": null,
    "hierarchyLevel": 2
  }
]
```

### 4.4 按 ID 查询区域

| 项目 | 内容 |
|---|---|
| 接口说明 | 根据区域 ID 查询单个区域 |
| HTTP Method | `GET` |
| URL | `/campus-areas/{id}` |

路径参数：

| 名称 | 类型 | 必填 | 描述 |
|---|---|---:|---|
| `id` | string | 是 | 区域 ID，例如 `demo-area-library` |

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | 单个 `CampusArea` 对象 |
| 404 | 区域不存在 | 文本：`未找到 ID 为 {id} 的校园区域。` |

### 4.5 查询直接下级区域

| 项目 | 内容 |
|---|---|
| 接口说明 | 查询指定区域的直接子区域，不递归返回更深层级 |
| HTTP Method | `GET` |
| URL | `/campus-areas/{id}/children` |

路径参数：

| 名称 | 类型 | 必填 | 描述 |
|---|---|---:|---|
| `id` | string | 是 | 父区域 ID |

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `CampusArea[]`，没有子区域时为 `[]` |
| 404 | 父区域不存在 | 文本：`未找到 ID 为 {id} 的校园区域。` |

### 4.6 新增区域

| 项目 | 内容 |
|---|---|
| 接口说明 | 新增校园区域，区域 ID 由后端生成 |
| HTTP Method | `POST` |
| URL | `/campus-areas` |

请求体：见 [CampusArea](#31-campusarea)。

```json
{
  "areaName": "南校区教学楼周边",
  "campusName": "四平路校区",
  "parentAreaID": "demo-area-siping",
  "areaType": "公共区域",
  "riskLevel": "低",
  "geoBoundary": null
}
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 201 | 创建成功 | 完整 `CampusArea` 对象，其中 `areaID` 为新 UUID |
| 400 | 名称为空或超过 100 字符 | 对应的业务错误文本 |
| 400 | 父区域不存在、指向自身或形成循环 | 对应的业务错误文本 |

### 4.7 更新区域

| 项目 | 内容 |
|---|---|
| 接口说明 | 完整更新指定校园区域 |
| HTTP Method | `PUT` |
| URL | `/campus-areas/{id}` |

路径参数：`id` 为待更新区域 ID。请求体结构同 `CampusArea`，可省略 `areaID`。

```json
{
  "areaName": "南校区教学楼周边（已更新）",
  "campusName": "四平路校区",
  "parentAreaID": "demo-area-siping",
  "areaType": "公共区域",
  "riskLevel": "中",
  "geoBoundary": null
}
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 更新成功 | 无 |
| 400 | URL 与请求体 ID 不一致 | 文本：`URL 中的区域 ID 与请求体中的区域 ID 不匹配。` |
| 400 | 其他业务校验失败 | 对应的业务错误文本 |
| 404 | 区域不存在 | 文本：`未找到 ID 为 {id} 的校园区域。` |

### 4.8 删除区域

| 项目 | 内容 |
|---|---|
| 接口说明 | 删除不存在下级区域且未被业务数据引用的区域 |
| HTTP Method | `DELETE` |
| URL | `/campus-areas/{id}` |

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 删除成功 | 无 |
| 404 | 区域不存在 | 文本：`未找到 ID 为 {id} 的校园区域。` |
| 409 | 仍包含下级区域 | 文本：`该区域仍包含下级区域，不能直接删除。` |
| 409 | 被猫咪、服务点、目击或紧急上报引用 | 文本：`该区域仍被猫咪、服务点、目击或紧急上报记录使用，不能删除。` |

## 5. 服务点接口

### 5.1 条件查询服务点

| 项目 | 内容 |
|---|---|
| 接口说明 | 查询服务点或猫窝，支持组合筛选 |
| HTTP Method | `GET` |
| URL | `/service-points` |

| 名称 | 类型 | 必填 | 描述 |
|---|---|---:|---|
| `areaId` | string | 否 | 所属区域 ID |
| `pointType` | string | 否 | 点位类型，例如“猫窝” |
| `facilityStatus` | string | 否 | 设施状态 |

请求示例：

```http
GET /service-points?areaId=demo-area-library&pointType=猫窝
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `ServicePoint[]`，无数据时为 `[]` |

```json
[
  {
    "pointID": "demo-point-library-nest",
    "areaID": "demo-area-library",
    "pointName": "图书馆北侧猫窝",
    "pointType": "猫窝",
    "longitude": 121.5063,
    "latitude": 31.2824,
    "facilityStatus": "需定期巡查",
    "deployTime": "2026-07-20T08:00:00"
  }
]
```

### 5.2 按 ID 查询服务点

| 项目 | 内容 |
|---|---|
| 接口说明 | 根据点位 ID 查询单个服务点 |
| HTTP Method | `GET` |
| URL | `/service-points/{id}` |

路径参数：`id` 为服务点 ID，例如 `demo-point-library-nest`。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | 单个 `ServicePoint` 对象 |
| 404 | 服务点不存在 | 文本：`未找到 ID 为 {id} 的服务点。` |

### 5.3 新增服务点

| 项目 | 内容 |
|---|---|
| 接口说明 | 新增服务点或猫窝，点位 ID 由后端生成 |
| HTTP Method | `POST` |
| URL | `/service-points` |

请求体：见 [ServicePoint](#32-servicepoint)。

```json
{
  "areaID": "demo-area-library",
  "pointName": "图书馆西侧猫窝",
  "pointType": "猫窝",
  "longitude": 121.5064,
  "latitude": 31.2822,
  "facilityStatus": "正常",
  "deployTime": "2026-08-03T08:00:00Z"
}
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 201 | 创建成功 | 完整 `ServicePoint` 对象，其中 `pointID` 为新 UUID |
| 400 | 点位名称为空或超过 100 字符 | 对应的业务错误文本 |
| 400 | 关联区域不存在 | 文本：`关联区域 {areaID} 不存在。` |
| 400 | 经纬度未同时提供或超出范围 | 对应的业务错误文本 |

### 5.4 更新服务点

| 项目 | 内容 |
|---|---|
| 接口说明 | 完整更新指定服务点 |
| HTTP Method | `PUT` |
| URL | `/service-points/{id}` |

路径参数：`id` 为待更新点位 ID。请求体结构同 `ServicePoint`，可省略 `pointID`。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 更新成功 | 无 |
| 400 | URL 与请求体 ID 不一致 | 文本：`URL 中的点位 ID 与请求体中的点位 ID 不匹配。` |
| 400 | 其他业务校验失败 | 对应的业务错误文本 |
| 404 | 服务点不存在 | 文本：`未找到 ID 为 {id} 的服务点。` |

### 5.5 删除服务点

| 项目 | 内容 |
|---|---|
| 接口说明 | 删除未被排班或维护记录引用的服务点 |
| HTTP Method | `DELETE` |
| URL | `/service-points/{id}` |

路径参数：`id` 为待删除服务点 ID。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 删除成功 | 无 |
| 404 | 服务点不存在 | 文本：`未找到 ID 为 {id} 的服务点。` |
| 409 | 服务点仍被引用 | 文本：`该服务点仍被排班或维护记录使用，不能删除。` |

## 6. 猫窝维护记录接口

### 6.1 条件查询维护记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 查询猫窝维护记录，支持组合筛选 |
| HTTP Method | `GET` |
| URL | `/nest-maintenance-records` |

| 名称 | 类型 | 必填 | 描述 |
|---|---|---:|---|
| `pointId` | string | 否 | 猫窝点位 ID |
| `damageLevel` | string | 否 | 损坏程度 |
| `from` | datetime | 否 | 巡查时间下限，包含该时刻 |
| `to` | datetime | 否 | 巡查时间上限，包含该时刻；不能早于 `from` |

请求示例：

```http
GET /nest-maintenance-records?pointId=demo-point-library-nest&damageLevel=轻微
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `NestMaintenanceRecord[]`，无数据时为 `[]` |
| 400 | `from` 晚于 `to` | 文本：`开始时间不能晚于结束时间。` |

```json
[
  {
    "maintenanceID": "demo-maintenance-001",
    "pointID": "demo-point-library-nest",
    "materialType": "保温箱",
    "checkTime": "2026-07-21T09:00:00",
    "weatherCondition": "晴",
    "damageLevel": "轻微",
    "actionType": "清理",
    "operatorUserID": "demo-user-zhaoqing",
    "nextCheckTime": "2026-07-28T09:00:00",
    "remark": "已更换垫材"
  }
]
```

### 6.2 按 ID 查询维护记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 根据维护记录 ID 查询单条记录 |
| HTTP Method | `GET` |
| URL | `/nest-maintenance-records/{id}` |

路径参数：`id` 为维护记录 ID，例如 `demo-maintenance-001`。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | 单个 `NestMaintenanceRecord` 对象 |
| 404 | 维护记录不存在 | 文本：`未找到 ID 为 {id} 的维护记录。` |

### 6.3 新增维护记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 新增猫窝巡查或维护记录，记录 ID 由后端生成 |
| HTTP Method | `POST` |
| URL | `/nest-maintenance-records` |

请求体：见 [NestMaintenanceRecord](#33-nestmaintenancerecord)。

```json
{
  "pointID": "demo-point-library-nest",
  "materialType": "保温箱",
  "checkTime": "2026-08-03T09:00:00Z",
  "weatherCondition": "晴",
  "damageLevel": "轻微",
  "actionType": "维修",
  "operatorUserID": "demo-user-zhaoqing",
  "nextCheckTime": "2026-08-10T09:00:00Z",
  "remark": "更换防水垫材"
}
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 201 | 创建成功 | 完整 `NestMaintenanceRecord` 对象，其中 `maintenanceID` 为新 UUID |
| 400 | 点位 ID 或维护动作为空 | 对应的业务错误文本 |
| 400 | 点位或操作用户不存在 | 对应的业务错误文本 |
| 400 | 下次巡查时间早于本次巡查时间 | 文本：`下次巡查时间不能早于本次巡查时间。` |

### 6.4 更新维护记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 完整更新指定维护记录 |
| HTTP Method | `PUT` |
| URL | `/nest-maintenance-records/{id}` |

路径参数：`id` 为维护记录 ID。请求体结构同 `NestMaintenanceRecord`，可省略 `maintenanceID`。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 更新成功 | 无 |
| 400 | URL 与请求体 ID 不一致 | 文本：`URL 中的维护记录 ID 与请求体中的 ID 不匹配。` |
| 400 | 其他业务校验失败 | 对应的业务错误文本 |
| 404 | 维护记录不存在 | 文本：`未找到 ID 为 {id} 的维护记录。` |

### 6.5 删除维护记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 删除指定维护记录 |
| HTTP Method | `DELETE` |
| URL | `/nest-maintenance-records/{id}` |

路径参数：`id` 为待删除维护记录 ID。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 删除成功 | 无 |
| 404 | 维护记录不存在 | 文本：`未找到 ID 为 {id} 的维护记录。` |

## 7. 猫咪目击记录接口

### 7.1 条件查询目击记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 查询猫咪目击记录，支持组合筛选，按目击时间倒序返回 |
| HTTP Method | `GET` |
| URL | `/cat-sightings` |

| 名称 | 类型 | 必填 | 描述 |
|---|---|---:|---|
| `catId` | string | 否 | 猫咪 ID |
| `areaId` | string | 否 | 目击区域 ID |
| `from` | datetime | 否 | 目击时间下限，包含该时刻 |
| `to` | datetime | 否 | 目击时间上限，包含该时刻；不能早于 `from` |

请求示例：

```http
GET /cat-sightings?catId=demo-cat-campus-001&areaId=demo-area-library&from=2026-07-01T00:00:00Z&to=2026-08-31T23:59:59Z
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `CatSighting[]`，无数据时为 `[]` |
| 400 | `from` 晚于 `to` | 文本：`开始时间不能晚于结束时间。` |

```json
[
  {
    "sightingID": "demo-sighting-001",
    "catID": "demo-cat-campus-001",
    "userID": "demo-user-zhaoqing",
    "areaID": "demo-area-library",
    "longitude": 121.50645,
    "latitude": 31.2822,
    "photoUrl": "https://example.invalid/demo-cat-sighting.jpg",
    "sightingTime": "2026-07-21T18:30:00",
    "remark": "精神状态正常，正在投喂点附近活动"
  }
]
```

### 7.2 按 ID 查询目击记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 根据目击记录 ID 查询单条记录 |
| HTTP Method | `GET` |
| URL | `/cat-sightings/{id}` |

路径参数：`id` 为目击记录 ID，例如 `demo-sighting-001`。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | 单个 `CatSighting` 对象 |
| 404 | 目击记录不存在 | 文本：`未找到 ID 为 {id} 的目击记录。` |

### 7.3 查询某只猫的最近目击

| 项目 | 内容 |
|---|---|
| 接口说明 | 按时间倒序返回某只猫最近的若干条目击记录 |
| HTTP Method | `GET` |
| URL | `/cat-sightings/recent/by-cat/{catId}` |

| 名称 | 位置 | 类型 | 必填 | 描述 |
|---|---|---|---:|---|
| `catId` | path | string | 是 | 猫咪 ID |
| `limit` | query | integer | 否 | 返回数量，默认 10，范围 1—100 |

请求示例：

```http
GET /cat-sightings/recent/by-cat/demo-cat-campus-001?limit=10
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 200 | 查询成功 | `CatSighting[]`，猫咪不存在或无目击记录时为 `[]` |
| 400 | `catId` 为空 | 文本：`猫咪 ID 不能为空。` |
| 400 | `limit` 超出范围 | 文本：`limit 必须在 1 到 100 之间。` |

### 7.4 新增目击记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 新增猫咪目击记录，记录 ID 由后端生成 |
| HTTP Method | `POST` |
| URL | `/cat-sightings` |

请求体：见 [CatSighting](#34-catsighting)。

```json
{
  "catID": "demo-cat-campus-001",
  "userID": "demo-user-zhaoqing",
  "areaID": "demo-area-library",
  "longitude": 121.5064,
  "latitude": 31.2822,
  "photoUrl": "https://example.invalid/sighting-20260803.jpg",
  "sightingTime": "2026-08-03T18:30:00Z",
  "remark": "猫咪状态正常"
}
```

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 201 | 创建成功 | 完整 `CatSighting` 对象，其中 `sightingID` 为新 UUID |
| 400 | 猫咪、用户或区域 ID 为空 | 对应的业务错误文本 |
| 400 | 关联猫咪、用户或区域不存在 | 对应的业务错误文本 |
| 400 | 经纬度未同时提供或超出范围 | 对应的业务错误文本 |

### 7.5 更新目击记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 完整更新指定目击记录 |
| HTTP Method | `PUT` |
| URL | `/cat-sightings/{id}` |

路径参数：`id` 为目击记录 ID。请求体结构同 `CatSighting`，可省略 `sightingID`。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 更新成功 | 无 |
| 400 | URL 与请求体 ID 不一致 | 文本：`URL 中的目击记录 ID 与请求体中的 ID 不匹配。` |
| 400 | 其他业务校验失败 | 对应的业务错误文本 |
| 404 | 目击记录不存在 | 文本：`未找到 ID 为 {id} 的目击记录。` |

### 7.6 删除目击记录

| 项目 | 内容 |
|---|---|
| 接口说明 | 删除未被失踪预警引用的目击记录 |
| HTTP Method | `DELETE` |
| URL | `/cat-sightings/{id}` |

路径参数：`id` 为待删除目击记录 ID。

| 状态码 | 描述 | 响应体 |
|---:|---|---|
| 204 | 删除成功 | 无 |
| 404 | 目击记录不存在 | 文本：`未找到 ID 为 {id} 的目击记录。` |
| 409 | 已被失踪预警引用 | 文本：`该目击记录已被失踪预警引用，不能删除。` |

## 8. 联调健康检查

健康检查不是赵晴业务模块的一部分，但交叉测试前可先调用它确认 Oracle 连接可用。

| 项目 | 内容 |
|---|---|
| HTTP Method | `GET` |
| URL | `/health` |

成功响应：

```json
{
  "database": "connected",
  "message": "数据库连接正常。"
}
```

| 状态码 | 描述 |
|---:|---|
| 200 | Oracle 数据库连接正常 |
| 500 | 未配置连接字符串或数据库连接失败；响应体包含 `database` 与 `message` |

## 9. Postman 交叉测试说明

1. 启动 Oracle，并依次执行 `database/create_tables.sql`、`database/insert_demo_data.sql`。
2. 启动 API：

   ```powershell
   dotnet run --project src/CampusStrayCatSystem.Core/CampusStrayCatSystem.Core.csproj --launch-profile http
   ```

3. 在 Postman 中导入 `assets/demo/ZhaoQing.postman_collection.json`。
4. 集合默认使用 `baseUrl=http://localhost:5047`，并使用以下演示外键：
   - `catId=demo-cat-campus-001`
   - `userId=demo-user-zhaoqing`
5. 按集合顺序执行。集合会依次创建测试区域、服务点、维护记录和目击记录，完成查询与更新后再逆序删除，避免外键冲突。
6. 交叉测试至少覆盖：空数组、无效外键、开始时间晚于结束时间、经纬度只提供一个、经纬度越界、URL/请求体 ID 不一致、删除被引用数据等场景。
