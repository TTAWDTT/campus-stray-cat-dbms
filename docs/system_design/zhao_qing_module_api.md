# 赵晴负责模块 API 说明

## 模块范围

赵晴负责校园区域、服务点与猫窝维护、目击记录三个功能模块。所有接口使用 `VARCHAR2(36)` 字符串 UUID 作为主键，创建接口由服务端生成主键。

## 校园区域

基础路径：`/api/campus-areas`

| 方法 | 路径 | 用途 |
|---|---|---|
| GET | `/api/campus-areas` | 查询区域，可使用 `campusName`、`areaType`、`riskLevel` 筛选 |
| GET | `/api/campus-areas/roots` | 查询所有根区域 |
| GET | `/api/campus-areas/hierarchy` | 按 Oracle 层级顺序查询全部区域，并返回 `hierarchyLevel` |
| GET | `/api/campus-areas/{id}` | 查询单个区域 |
| GET | `/api/campus-areas/{id}/children` | 查询指定区域的直接下级区域 |
| POST | `/api/campus-areas` | 新增区域 |
| PUT | `/api/campus-areas/{id}` | 更新区域 |
| DELETE | `/api/campus-areas/{id}` | 删除不包含下级区域的区域 |

新增区域至少需要提供 `areaName`。如提供 `parentAreaID`，接口会检查父区域是否存在并拒绝循环层级。仍包含下级区域或被其他业务记录引用的区域不能删除。

## 服务点与猫窝维护

服务点基础路径：`/api/service-points`

| 方法 | 路径 | 用途 |
|---|---|---|
| GET | `/api/service-points` | 查询点位，可使用 `areaId`、`pointType`、`facilityStatus` 筛选 |
| GET | `/api/service-points/{id}` | 查询单个点位 |
| POST | `/api/service-points` | 新增点位 |
| PUT | `/api/service-points/{id}` | 更新点位 |
| DELETE | `/api/service-points/{id}` | 删除点位 |

猫窝维护基础路径：`/api/nest-maintenance-records`

| 方法 | 路径 | 用途 |
|---|---|---|
| GET | `/api/nest-maintenance-records` | 查询维护记录，可使用 `pointId`、`damageLevel`、`from`、`to` 筛选 |
| GET | `/api/nest-maintenance-records/{id}` | 查询单条维护记录 |
| POST | `/api/nest-maintenance-records` | 新增维护记录 |
| PUT | `/api/nest-maintenance-records/{id}` | 更新维护记录 |
| DELETE | `/api/nest-maintenance-records/{id}` | 删除维护记录 |

点位经纬度必须同时提供，经度范围为 `-180` 至 `180`，纬度范围为 `-90` 至 `90`。维护记录必须关联已存在的点位，并且下次巡查时间不能早于本次巡查时间；被排班或维护记录引用的点位不能删除。

## 目击记录

基础路径：`/api/cat-sightings`

| 方法 | 路径 | 用途 |
|---|---|---|
| GET | `/api/cat-sightings` | 查询目击记录，可使用 `catId`、`areaId`、`from`、`to` 筛选 |
| GET | `/api/cat-sightings/{id}` | 查询单条目击记录 |
| GET | `/api/cat-sightings/recent/by-cat/{catId}?limit=10` | 查询某只猫最近的目击记录，`limit` 范围为 1—100 |
| POST | `/api/cat-sightings` | 新增目击记录 |
| PUT | `/api/cat-sightings/{id}` | 更新目击记录 |
| DELETE | `/api/cat-sightings/{id}` | 删除目击记录 |

新增目击记录必须提供 `catID`、`userID` 和 `areaID`。接口会先检查关联的猫咪、用户和区域是否存在；未提供 `sightingTime` 时使用服务器当前 UTC 时间。

## 本地联调

1. 配置 `src/CampusStrayCatSystem.Core/appsettings.json` 中的 Oracle 连接字符串。
2. 依次运行 `database/create_tables.sql` 和 `database/insert_demo_data.sql`。
3. 执行 `dotnet run --project src/CampusStrayCatSystem.Core/CampusStrayCatSystem.Core.csproj --launch-profile http`。
4. 打开 `http://localhost:5047/swagger`，或导入 `assets/demo/ZhaoQing.postman_collection.json`。
5. 需要直接验证 SQL 时运行 `database/queries/test_queries.sql`。
