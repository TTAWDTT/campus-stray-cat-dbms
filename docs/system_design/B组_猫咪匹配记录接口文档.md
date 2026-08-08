# B组猫咪匹配记录接口说明

## 模块范围

本模块读取外部识别流程写入的 `CAT_MATCHRECORDS`，为登录用户提供来源照片的候选猫列表和匹配详情，并允许管理员或志愿者确认候选结果。本阶段不生成特征向量、不执行相似度计算，也不提供匹配记录创建接口。

所有接口都需要 `Authorization: Bearer <JWT>`。确认接口还要求 JWT 角色为 `ADMIN` 或 `VOLUNTEER`。

## 接口总览

| 方法 | 路径 | 用途 | 成功状态 |
|---|---|---|---|
| GET | `/api/cats/{catId}/photos/{photoId}/matches` | 查询来源照片的候选匹配 | `200 OK` |
| GET | `/api/cat-matches/{matchId}` | 查询单条匹配详情 | `200 OK` |
| PATCH | `/api/cat-matches/{matchId}/confirmation` | 确认或拒绝候选结果 | `204 No Content` |

## 匹配记录响应

```json
{
  "matchID": "demo-match-record-001",
  "sourcePhotoID": "demo-match-source-001",
  "candidateCatID": "demo-cat-match-002",
  "similarityScore": 91.25,
  "rankNo": 1,
  "confirmStatus": "PENDING",
  "confirmUserID": null,
  "sourcePhotoUrl": "/uploads/cats/demo-cat-campus-001/demo-match-source-001.jpg",
  "candidateCatName": "匹配候选二号",
  "candidateArchiveStatus": "PUBLISHED",
  "candidateAreaName": "图书馆周边",
  "candidatePrimaryPhotoUrl": "/uploads/cats/demo-cat-match-002/demo-match-candidate-002-primary.jpg"
}
```

`confirmStatus` 在响应中始终是 `PENDING`、`CONFIRMED` 或 `REJECTED`。数据库中的 `NULL` 状态按 `PENDING` 返回；`similarityScore`、`rankNo` 和关联扩展字段可以为空。

## 查询来源照片的匹配

- Method：`GET`
- URL：`/api/cats/{catId}/photos/{photoId}/matches`
- Query 参数：
  - `candidateCatId`：可选，按候选猫 ID 精确筛选。
  - `confirmStatus`：可选，支持 `PENDING`、`CONFIRMED`、`REJECTED`，大小写和首尾空格不敏感。
- 排序：`rankNo` 非空优先并按升序；排名相同时相似度非空优先并按降序；最后按 `matchID` 升序。

成功返回匹配记录数组。照片不存在或照片不属于 URL 中的猫咪返回 `404 Not Found`；路径 ID、候选猫 ID 或状态非法返回 `400 Bad Request`。

## 查询单条匹配

- Method：`GET`
- URL：`/api/cat-matches/{matchId}`

匹配记录不存在返回 `404 Not Found`；记录 ID 为空、超过 36 字节或包含不安全字符返回 `400 Bad Request`。

## 确认匹配

- Method：`PATCH`
- URL：`/api/cat-matches/{matchId}/confirmation`
- Content-Type：`application/json`

请求体只能是：

```json
{ "confirmStatus": "CONFIRMED" }
```

或：

```json
{ "confirmStatus": "REJECTED" }
```

服务端从 JWT 的 `NameIdentifier` Claim 取得确认人，不接受客户端传入的用户 ID。更新在 Oracle 事务中锁定目标记录，并再次确认来源照片和候选猫仍存在。

- `204 No Content`：更新成功。
- `400 Bad Request`：ID 或状态非法，或状态为空、为 `PENDING`。
- `401 Unauthorized`：登录身份缺失或无效。
- `403 Forbidden`：登录用户不是管理员或志愿者。
- `404 Not Found`：匹配记录不存在。
- `409 Conflict`：匹配记录的照片或候选猫关联已失效。

重复确认同一状态和重新选择另一决策状态都按普通更新处理；本阶段不限制同一来源照片只能有一条 `CONFIRMED` 记录。

## 数据库约束与脚本

新建数据库时，`database/create_tables.sql` 会创建以下约束：

- `CK_CAT_MATCH_SCORE`：相似度为 `0` 到 `100`，或为空。
- `CK_CAT_MATCH_RANK`：排名不小于 `1`，或为空。
- `CK_CAT_MATCH_STATUS`：状态为 `PENDING`、`CONFIRMED`、`REJECTED`，或为空。
- `UQ_CAT_MATCH_SOURCE_CANDIDATE`：同一来源照片和候选猫组合不能重复。
- `UQ_CAT_MATCH_SOURCE_RANK`：同一来源照片的非空排名不能重复。

已有数据库执行 `database/queries/cat_matches_oracle_programming.sql`。脚本会先检查异常数据和重复数据；发现问题时停止并提示人工修复，不会继续添加约束。验收使用 `database/queries/cat_matches_acceptance.sql`，演示数据使用 `database/queries/cat_matches_demo_data.sql`。

## 本地联调

1. 执行 `database/queries/a_group_demo_data.sql`，准备 `a_group_volunteer` 账号，密码为 `Passw0rd!`。
2. 执行 `database/insert_demo_data.sql` 和 `database/queries/cat_matches_demo_data.sql`。
3. 启动后端：

   ```bash
   dotnet run --project src/CampusStrayCatSystem.Core/CampusStrayCatSystem.Core.csproj --launch-profile http
   ```

4. 导入 `docs/system_design/B组_猫咪匹配记录Postman集合.json`，先运行登录请求，再运行列表、筛选、详情和确认请求。
